// Server.cpp : definisce il punto di ingresso dell'applicazione.
//

#include "stdafx.h"
#include "Server.h"
#include "GWindow.h"

#define WM_USER_SHELLICON WM_USER + 1
#define MAX_LOADSTRING 100

#define DEFAULT_PORT "27015"
#define DEFAULT_BUFLEN 8192

// DEBUG
bool			debug = false;

// Variabili globali:
HWND			MALPROJWnd;							// WFinestra di questa applicazione
HINSTANCE		hInst;                              // istanza corrente
NOTIFYICONDATA	nidApp;
HMENU			hPopMenu;
WCHAR			szTitle[MAX_LOADSTRING];			// Testo della barra del titolo
WCHAR			szWindowClass[MAX_LOADSTRING];		// nome della classe di finestre principale
TCHAR			applicationToolTip[MAX_LOADSTRING];
BOOL			hRunning = TRUE;
bool			waiting = false;
bool			enable_response_msg = true;

// Variabili globali per il thread:
std::atomic<BOOL>	stop_communication_thread(FALSE);	// Avvia/ferma il communication thread
std::thread			*communication_thread;

// Variabili globali per le finestre:
std::map<std::pair<HWND, DWORD>, GWindow>	windows;
HWND										window_in_focus = NULL;


// Dichiarazioni con prototipo delle funzioni incluse in questo modulo di codice:
ATOM                MyRegisterClass(HINSTANCE hInstance);
BOOL                InitInstance(HINSTANCE, int);
LRESULT CALLBACK    WndProc(HWND, UINT, WPARAM, LPARAM);
INT_PTR CALLBACK    About(HWND, UINT, WPARAM, LPARAM);

// Prototipo della funzione usata per la connessione
void				communication_func();

// Prototipi delle funzioni usate per la funzionalità COMMAND
int					compute_command(char* command, int command_len, char* output);
int					identify_key(char* key);
BOOL				do_command(HWND hWnd, int* tasti, int num_tasti);
int					sendn(SOCKET sock, char* sendbuf, size_t bufsize);

// Prototipi delle funzioni usate per il Monitor delle finestre
BOOL CALLBACK		EnumWindowsProc(HWND hWnd, LPARAM lParam);




//------------------------------------------------------------------------------------
// FUNZIONI INTERFACCIA GRAFICA

int APIENTRY wWinMain(_In_ HINSTANCE hInstance,
                     _In_opt_ HINSTANCE hPrevInstance,
                     _In_ LPWSTR    lpCmdLine,
                     _In_ int       nCmdShow)
{
    UNREFERENCED_PARAMETER(hPrevInstance);
    UNREFERENCED_PARAMETER(lpCmdLine);

    // TODO: inserire qui il codice.

	// Lancio il thread per la comunicazione client-server
	communication_thread = new std::thread(communication_func);

    // Inizializzare le stringhe globali
    LoadStringW(hInstance, IDS_APP_TITLE, szTitle, MAX_LOADSTRING);
    LoadStringW(hInstance, IDC_SERVER, szWindowClass, MAX_LOADSTRING);
    MyRegisterClass(hInstance);

    // Eseguire l'inizializzazione dall'applicazione:
    if (!InitInstance (hInstance, nCmdShow))
    {
		stop_communication_thread.store(TRUE);
		communication_thread->join();
        return FALSE;
    }

    HACCEL hAccelTable = LoadAccelerators(hInstance, MAKEINTRESOURCE(IDC_SERVER));

    MSG msg;

    // Ciclo di messaggi principale:
    while (GetMessage(&msg, nullptr, 0, 0))
    {
        if (!TranslateAccelerator(msg.hwnd, hAccelTable, &msg))
        {
            TranslateMessage(&msg);
            DispatchMessage(&msg);
        }
    }

    return (int) msg.wParam;
}



//
//  FUNZIONE: MyRegisterClass()
//
//  SCOPO: registra la classe di finestre.
//
ATOM MyRegisterClass(HINSTANCE hInstance)
{
    WNDCLASSEXW wcex;

    wcex.cbSize = sizeof(WNDCLASSEX);

    wcex.style          = CS_HREDRAW | CS_VREDRAW;
    wcex.lpfnWndProc    = WndProc;
    wcex.cbClsExtra     = 0;
    wcex.cbWndExtra     = 0;
    wcex.hInstance      = hInstance;
    wcex.hIcon          = LoadIcon(hInstance, MAKEINTRESOURCE(IDI_SERVER));
    wcex.hCursor        = LoadCursor(nullptr, IDC_ARROW);
    wcex.hbrBackground  = (HBRUSH)(COLOR_WINDOW+1);
    wcex.lpszMenuName   = MAKEINTRESOURCEW(IDC_SERVER);
    wcex.lpszClassName  = szWindowClass;
    wcex.hIconSm        = LoadIcon(wcex.hInstance, MAKEINTRESOURCE(IDI_SMALL));

    return RegisterClassExW(&wcex);
}

//
//   FUNZIONE: InitInstance(HINSTANCE, int)
//
//   SCOPO: salva l'handle di istanza e crea la finestra principale
//
//   COMMENTI:
//
//        In questa funzione l'handle di istanza viene salvato in una variabile globale e
//        la finestra di programma principale viene creata e visualizzata.
//
BOOL InitInstance(HINSTANCE hInstance, int nCmdShow)
{
   hInst = hInstance; // Memorizzare l'handle di istanza nella variabile globale

   MALPROJWnd = CreateWindowW(szWindowClass, szTitle, WS_OVERLAPPEDWINDOW,
      CW_USEDEFAULT, 0, CW_USEDEFAULT, 0, nullptr, nullptr, hInstance, nullptr);

   if (!MALPROJWnd)
   {
      return FALSE;
   }

   HICON hMainIcon = LoadIcon(hInst, (LPCWSTR)MAKEINTRESOURCE(IDI_SERVER));

   nidApp.cbSize = sizeof(NOTIFYICONDATA);
   nidApp.hWnd = (HWND)MALPROJWnd;
   nidApp.uID = IDI_SERVER;
   nidApp.uFlags = NIF_ICON | NIF_MESSAGE | NIF_TIP;
   nidApp.hIcon = hMainIcon;
   nidApp.uCallbackMessage = WM_USER_SHELLICON;
   LoadString(hInst, IDS_APPTOOLTIP, nidApp.szTip, MAX_LOADSTRING);
   Shell_NotifyIcon(NIM_ADD, &nidApp);

   return TRUE;
}

//
//  FUNZIONE: WndProc(HWND, UINT, WPARAM, LPARAM)
//
//  SCOPO:  elabora i messaggi per la finestra principale.
//
//  WM_COMMAND - elabora il menu dell'applicazione
//  WM_PAINT - disegna la finestra principale
//  WM_DESTROY - inserisce un messaggio di uscita e restituisce un risultato
//
//
POINT lpClickPoint;
LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	int vmId, wmEvent;

    switch (message)
    {
	case WM_USER_SHELLICON:
		// Message callback
		switch (LOWORD(lParam))
		{
		case WM_RBUTTONDOWN:
			UINT uFlag = MF_BYPOSITION | MF_STRING;
			GetCursorPos(&lpClickPoint);
			hPopMenu = CreatePopupMenu();
			InsertMenu(hPopMenu, 0xFFFFFFFF, MF_BYPOSITION | MF_STRING, IDM_ABOUT, _T("About"));
			InsertMenu(hPopMenu, 0xFFFFFFFF, MF_SEPARATOR, IDM_SEP, _T("SEP"));
			if (hRunning == TRUE) {
				InsertMenu(hPopMenu, 0xFFFFFFFF, MF_BYPOSITION | MF_STRING, IDM_INTERROMPI, _T("Interrompi"));
			}
			else {
				InsertMenu(hPopMenu, 0xFFFFFFFF, MF_BYPOSITION | MF_STRING, IDM_AVVIA, _T("Avvia"));
			}
			InsertMenu(hPopMenu, 0xFFFFFFFF, MF_SEPARATOR, IDM_SEP, _T("SEP"));
			InsertMenu(hPopMenu, 0xFFFFFFFF, MF_BYPOSITION | MFT_STRING, IDM_EXIT, _T("Esci"));

			SetForegroundWindow(hWnd);
			TrackPopupMenu(hPopMenu, TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_BOTTOMALIGN, lpClickPoint.x, lpClickPoint.y, 0, hWnd, NULL);
			
			return TRUE;
		}
		break;

    case WM_COMMAND:
        {
            int wmId = LOWORD(wParam);
            // Analizzare le selezioni di menu:
            switch (wmId)
            {
            case IDM_ABOUT:
				// Visualizza una finestra di informazioni
                DialogBox(hInst, MAKEINTRESOURCE(IDD_ABOUTBOX), hWnd, About);
                break;
			case IDM_AVVIA:
				// Avvia il server
				stop_communication_thread.store(FALSE);
				communication_thread = new std::thread(communication_func);
				hRunning = TRUE;
				break;
			case IDM_INTERROMPI:
				// Interrompe il server
				stop_communication_thread.store(TRUE);

				{	// Update della GUI per far si che l'utente non clicchi più volte sui comandi in una fase di stallo
					UINT uFlag = MF_BYPOSITION | MF_STRING | MF_GRAYED;		// Tutti i tasti sono resi "non cliccabili"
					hPopMenu = CreatePopupMenu();
					InsertMenu(hPopMenu, 0xFFFFFFFF, uFlag, IDM_ABOUT, _T("About"));
					InsertMenu(hPopMenu, 0xFFFFFFFF, MF_SEPARATOR, IDM_SEP, _T("SEP"));
					InsertMenu(hPopMenu, 0xFFFFFFFF, uFlag, IDM_INTERROMPI, _T("Attendi..."));	// Bottone "Interrompi" diventa "Attendi..."
					InsertMenu(hPopMenu, 0xFFFFFFFF, MF_SEPARATOR, IDM_SEP, _T("SEP"));
					InsertMenu(hPopMenu, 0xFFFFFFFF, uFlag, IDM_EXIT, _T("Esci"));

					SetForegroundWindow(hWnd);
					TrackPopupMenu(hPopMenu, TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_BOTTOMALIGN , lpClickPoint.x, lpClickPoint.y, 0, hWnd, NULL);
				}

				communication_thread->join();

				// PostMessage(hWnd, WM_KEYDOWN, VK_ESCAPE, 0);	// Questo non funziona... lo volevo usare per chiudere la finestra
				// Non funziona:
				// - ShowWindow(hWnd, SW_HIDE),
				// - neanche le funzione per distruggere i menu

				hRunning = FALSE;
				break;

            case IDM_EXIT:
				// Termina l'applicazione server
				if (hRunning == TRUE) {
					stop_communication_thread.store(TRUE);

					{	// Update della GUI per far si che l'utente non clicchi più volte sui comandi in una fase di stallo
						UINT uFlag = MF_BYPOSITION | MF_STRING | MF_GRAYED;		// Tutti i tasti sono resi "non cliccabili"
						hPopMenu = CreatePopupMenu();
						InsertMenu(hPopMenu, 0xFFFFFFFF, uFlag, IDM_ABOUT, _T("About"));
						InsertMenu(hPopMenu, 0xFFFFFFFF, MF_SEPARATOR, IDM_SEP, _T("SEP"));
						InsertMenu(hPopMenu, 0xFFFFFFFF, uFlag, IDM_INTERROMPI, _T("Interrompi"));
						InsertMenu(hPopMenu, 0xFFFFFFFF, MF_SEPARATOR, IDM_SEP, _T("SEP"));
						InsertMenu(hPopMenu, 0xFFFFFFFF, uFlag, IDM_EXIT, _T("Attendi..."));	// Bottone "Esci" diventa "Attendi..."

						SetForegroundWindow(hWnd);
						TrackPopupMenu(hPopMenu, TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_BOTTOMALIGN, lpClickPoint.x, lpClickPoint.y, 0, hWnd, NULL);
					}

					communication_thread->join();
				}

				Shell_NotifyIcon(NIM_DELETE, &nidApp);
				DestroyWindow(hWnd);
                break;
            default:
                return DefWindowProc(hWnd, message, wParam, lParam);
            }
        }
        break;
	/*
    case WM_PAINT:
        {
            PAINTSTRUCT ps;
            HDC hdc = BeginPaint(hWnd, &ps);
            // TODO: aggiungere qui il codice di disegno che usa HDC...
            EndPaint(hWnd, &ps);
        }
        break;
	*/
    case WM_DESTROY:
        PostQuitMessage(0);
        break;
    default:
        return DefWindowProc(hWnd, message, wParam, lParam);
    }
    return 0;
}

// Gestore dei messaggi della finestra Informazioni su.
INT_PTR CALLBACK About(HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam)
{
    UNREFERENCED_PARAMETER(lParam);
    switch (message)
    {
    case WM_INITDIALOG:
        return (INT_PTR)TRUE;

    case WM_COMMAND:
        if (LOWORD(wParam) == IDOK || LOWORD(wParam) == IDCANCEL)
        {
            EndDialog(hDlg, LOWORD(wParam));
            return (INT_PTR)TRUE;
        }
        break;
    }
    return (INT_PTR)FALSE;
}


//------------------------------------------------------------------------------------
// FUNZIONI COMUNICAZIONE

// Funzione principale del thread che avvia una comunicazione con un client remoto
void communication_func() {
	
	if(debug) MessageBox(NULL, _T("Communication thread avviato!"), _T("DEBUG"), MB_OK);

	// Creazione di una cartella per le icone
	LPCTSTR lpPathName = L"C:\\MALPROJIcons";
	auto return_value_dir = CreateDirectory(lpPathName, (LPSECURITY_ATTRIBUTES)NULL);
	if (GetLastError() != ERROR_ALREADY_EXISTS && return_value_dir == 0) {
		if(debug) MessageBox(NULL, _T("Non posso creare una cartella nel disco C."), _T("Errore creazione cartella"), MB_OK | MB_ICONERROR);
		hRunning = FALSE;
		stop_communication_thread.store(true);
		return;
	}

	while (!stop_communication_thread.load()) {

		// Pulizia della struttura dati delle finestre
		windows.clear();
		
		// Variabili
		WSADATA wsaData;
		int iResult, iSendResult;
		struct addrinfo *result = NULL, *ptr = NULL, hints;

		SOCKET ListenSocket = INVALID_SOCKET;
		SOCKET ClientSocket = INVALID_SOCKET;

		char recvbuf[DEFAULT_BUFLEN];
		int recvbuflen = DEFAULT_BUFLEN;

		char sendvbuf[DEFAULT_BUFLEN];
		int sendvbuflen = DEFAULT_BUFLEN;
		
		// Inizializzazione di WinSock
		iResult = WSAStartup(MAKEWORD(2, 2), &wsaData);
		if (iResult != 0) {
			if(debug) MessageBox(NULL, _T("Necessaria una nuova connessione con il client!"), _T("Errore durante la comunicazione"), MB_OK | MB_ICONERROR);
			continue;
		}

		ZeroMemory(&hints, sizeof(hints));
		hints.ai_family = AF_INET;
		hints.ai_socktype = SOCK_STREAM;
		hints.ai_protocol = IPPROTO_TCP;
		hints.ai_flags = AI_PASSIVE;

		// Risoluzione dell'indirizzo locale e della porta che verrà usata dal server
		iResult = getaddrinfo(NULL, DEFAULT_PORT, &hints, &result);
		if (iResult != 0) {
			WSACleanup();
			if(debug) MessageBox(NULL, _T("Necessaria una nuova connessione con il client!"), _T("Errore durante la comunicazione"), MB_OK | MB_ICONERROR);
			continue;
		}

		// Creazione di un SOCKET
		ListenSocket = socket(result->ai_family, result->ai_socktype, result->ai_protocol);
		if (ListenSocket == INVALID_SOCKET) {
			freeaddrinfo(result);
			WSACleanup();
			if(debug) MessageBox(NULL, _T("Necessaria una nuova connessione con il client!"), _T("Errore durante la comunicazione"), MB_OK | MB_ICONERROR);
			continue;
		}

		// Setup del SOCKET TCP
		iResult = bind(ListenSocket, result->ai_addr, (int)result->ai_addrlen);
		if (iResult == SOCKET_ERROR) {
			freeaddrinfo(result);
			closesocket(ListenSocket);
			WSACleanup();
			if(debug) MessageBox(NULL, _T("Necessaria una nuova connessione con il client!"), _T("Errore durante la comunicazione"), MB_OK | MB_ICONERROR);
			continue;
		}

		freeaddrinfo(result);	// Non necessario da ora in poi

		u_long mode = 1; // abilita il socket non-bloccante
		if (ioctlsocket(ListenSocket, FIONBIO, &mode) != NO_ERROR) {
			freeaddrinfo(result);
			closesocket(ListenSocket);
			WSACleanup();
			if(debug) MessageBox(NULL, _T("Necessaria una nuova connessione con il client!"), _T("Errore durante la comunicazione"), MB_OK | MB_ICONERROR);
			continue;
		}

		int listen_error = 0;
		iResult = listen(ListenSocket, SOMAXCONN);
		if (iResult == SOCKET_ERROR) {
			closesocket(ListenSocket);
			WSACleanup();
			if(debug) MessageBox(NULL, _T("Necessaria una nuova connessione con il client!"), _T("Errore durante la comunicazione"), MB_OK | MB_ICONERROR);
			continue;
		}

		// Abilita la ricezione di richieste in entrata
		do {
			ClientSocket = accept(ListenSocket, NULL, NULL);
		} while (ClientSocket == INVALID_SOCKET && (!stop_communication_thread.load()) );

		if (stop_communication_thread.load()) {
			closesocket(ListenSocket);
			WSACleanup();
			break;
		}

		closesocket(ListenSocket);	// Non necessario da ora in poi, il socket può essere chiuso

		/*
		// Setta il socket del client in modalità bloccante
		mode = 0; // Abilita il socket bloccante
		if (ioctlsocket(ClientSocket, FIONBIO, &mode) != NO_ERROR) {
			freeaddrinfo(result);
			closesocket(ClientSocket);
			WSACleanup();
			if (debug) MessageBox(NULL, _T("Non ho potuto settare il socket client in modalità bloccante"), _T("Errore durante la comunicazione"), MB_OK | MB_ICONERROR);
			continue;
		}
		*/

		// Ricevi fino a che il client non interrompe la connessione
		int error_flag = 0;

		do {

			iResult = recv(ClientSocket, recvbuf, recvbuflen, 0);
			if (iResult > 0) {	// Il client ha mandato un comando o un messaggio di interruzione

				char output[DEFAULT_BUFLEN];
				int output_len = compute_command(recvbuf, recvbuflen, output);

				if (output_len == -1) { // Stop message
					if(enable_response_msg) iSendResult = sendn(ClientSocket, "stop", 5);
					break;	// Esce dal loop while
				}
				else if (output_len == 0) {	// Error
					if(enable_response_msg) iSendResult = sendn(ClientSocket, "error", 6);
				}
				else if (output_len > 0){	// Command message accettato
					if(enable_response_msg) iSendResult = sendn(ClientSocket, "ok", 3);
				}

				// Checking error
				if (iSendResult == SOCKET_ERROR && enable_response_msg) {
					error_flag = 1;
					break;
				}
			}
			
			// Enumerazione delle finestre
			std::vector<std::pair<HWND,DWORD>> newWins;
			std::vector<GWindow> oldWins;

			// Setta a FALSE il field "checked" di tutte le finestre
			for (auto win = windows.begin(); win != windows.end(); ++win) {
				win->second.checked = FALSE;
				win->second.newWin = FALSE;
			}

			// Finestra in focus
			HWND new_window_in_focus = GetForegroundWindow();
			BOOL new_focus;
			if (new_window_in_focus != window_in_focus) {
				new_focus = TRUE;
				window_in_focus = new_window_in_focus;
			}
			else new_focus = FALSE;
			GWindow Gwin_in_focus;

			// Enumerazione delle finestre attive in questo momento
			EnumWindows(EnumWindowsProc, NULL);

			// Raccolta informazioni finestre chiuse, finestre nuove e finestra in focus
			for (auto win_it = windows.begin(); win_it != windows.end(); ++win_it) {
				if (win_it->second.newWin) newWins.push_back(win_it->first);				// Nuova finestra da mandare
				else if (!win_it->second.checked) oldWins.push_back(win_it->second);		// Finestra chiusa da mandare

				if (new_focus && window_in_focus == win_it->second.hwnd)
					Gwin_in_focus = win_it->second;
			}

			// Invia le informazioni riguardanti le nuove finestre
			if (newWins.size() > 0) {
				sprintf_s(sendvbuf, "windows %d", newWins.size());
				while (sendn(ClientSocket, sendvbuf, strlen(sendvbuf) + 1) != 0 && !stop_communication_thread.load()) Sleep(10);
				if(waiting) Sleep(200);

				for (auto win_it = newWins.begin(); win_it != newWins.end() && !stop_communication_thread.load(); ++win_it) {
					int sendvbuf_tmp_size;

					auto wndfound = windows.find(std::pair<HWND, DWORD>(win_it->first, win_it->second));

					if (wndfound != windows.end()) {
						char* tmp_sendvbuf = wndfound->second.all_to_string(sendvbuf_tmp_size);
						memcpy_s(sendvbuf, BUFFER_LEN, tmp_sendvbuf, sendvbuf_tmp_size);
						while (sendn(ClientSocket, sendvbuf, sendvbuf_tmp_size) != 0 && !stop_communication_thread.load()) Sleep(10);
						if(waiting) Sleep(200);
					}
				}
			}

			// Invia le informazioni riguardanti le finestre chiuse
			if (oldWins.size() > 0) {
				sprintf_s(sendvbuf, "closed %d\n", oldWins.size());
				for (auto win_it = oldWins.begin(); win_it != oldWins.end() && !stop_communication_thread.load(); ++win_it) {
					sprintf_s(sendvbuf, "closed 1\n%d %d\n", win_it->hwnd, win_it->process_id);
					while (sendn(ClientSocket, sendvbuf, strlen(sendvbuf) + 1) != 0 && !stop_communication_thread.load()) Sleep(10);
					if(waiting) Sleep(200);
				}
			}

			// Invia le info sulla nuova finestra in focus (se presente)
			if (new_focus) {
				
				if (window_in_focus == NULL) sprintf_s(sendvbuf, "focus nullwindow\n");
				else if (Gwin_in_focus.process_id == 0) sprintf_s(sendvbuf, "focus windowsoperatingsystem\n");
				else sprintf_s(sendvbuf, "focus %d %d", Gwin_in_focus.hwnd, Gwin_in_focus.process_id);

				while (sendn(ClientSocket, sendvbuf, strlen(sendvbuf)+1) != 0 || stop_communication_thread.load()) Sleep(10);
				if(waiting) Sleep(200);
			} 

			// Elimina le vecchie finestre dalla struttura dati (checked = FALSE)
			auto map_it = windows.begin();
			while (map_it != windows.end() && !stop_communication_thread.load()) {
				if (map_it->second.checked == FALSE) {
					DestroyIcon(map_it->second.icon[0]);
					if (map_it->second.hwnd != MALPROJWnd) {}	// Controllo di sicurezza che non dovrebbe mai essere falso
						//CloseHandle(map_it->second.hwnd);
					map_it = windows.erase(map_it);
				}
				else {
					++map_it;
				}
			}

		} while (!stop_communication_thread.load());

		if(debug) MessageBox(NULL, _T("Comunicazione con il client interrotta!"), _T("DEBUG"), MB_OK);

		// Se non ci sono stati errori chiudi la connessione
		if (!error_flag) shutdown(ClientSocket, SD_SEND);

		// Cleanup
		closesocket(ClientSocket);
		WSACleanup();
	}

	if(debug) MessageBox(NULL, _T("Communication thread interrotto!"), _T("DEBUG"), MB_OK );
}


int	sendn(SOCKET sock, char* sendbuf, size_t bufsize) {

	char*	sendvbuf;
	char*	orig_sendvbuf;
	size_t	sendvbuflen = BUFFER_LEN;
	size_t	nleft;	size_t nwritten;

	if (sendbuf == NULL || bufsize == 0) return -255;
	if (bufsize > sendvbuflen) return -255;
	
	sendvbuf = (char*)malloc(sendvbuflen * sizeof(char));
	if (sendvbuf == NULL) return -255;
	memset(sendvbuf, 0, sendvbuflen*sizeof(char));

	memcpy_s(sendvbuf, sendvbuflen, sendbuf, bufsize);

	orig_sendvbuf = sendvbuf;
	for (nleft = sendvbuflen; nleft > 0;) {
		nwritten = send(sock, sendvbuf, nleft, 0);
		if (nwritten <= 0) {
			// Error
			free(sendvbuf);
			return (nwritten);
		}
		else {
			nleft -= nwritten;
			sendvbuf += nwritten;
		}
	}

	free(orig_sendvbuf);

	// Buffer per la risposta del client
	char* recvbuf = (char*)malloc(sendvbuflen * sizeof(char));
	if (recvbuf == NULL) return -255;

	while ( recv(sock, recvbuf, sendvbuflen, 0) <= 0 && !stop_communication_thread.load() );	// Aspetta la risposta del client
	if (recvbuf[0] != 'o' || recvbuf[1] != 'k') {
		free(recvbuf);
		return -255;
	}

	free(recvbuf);
	
	return (nleft);
}

//------------------------------------------------------------------------------------
// FUNZIONI INTERPRETAZIONE ED ESECUZIONE COMANDI

int	compute_command(char* command, int command_len, char* output) {

	if (debug) {
		wchar_t* wstring = new wchar_t[4096];
		MultiByteToWideChar(CP_ACP, 0, command, -1, wstring, 4096);
		if(debug) MessageBox(NULL, wstring, _T("Comando ricevuto"), MB_OK);
	}

	if (strncmp(command, "command", 7) == 0) {
		
		// Tokenizza ed esegue il comando

		HWND hwnd;
		char* window_handle;
		DWORD pid;

		unsigned int num_tasti;
		int* tasti;
		
		char* next_token = NULL;
		char* parsing_res = strtok_s(command, " ", &next_token);	// parsing_res == "command"
		if (parsing_res == NULL) return 0;
		
		window_handle = strtok_s(NULL, " ", &next_token);			// window_handle == "<window handle>"
		if (window_handle == NULL) return 0;
		hwnd = (HWND) atoi(window_handle);

		parsing_res = strtok_s(NULL, " ", &next_token);				// parsing_res = "<process id>"
		if (parsing_res == NULL) return 0;
		pid = (DWORD) atoi(parsing_res);
		
		parsing_res = strtok_s(NULL, " ", &next_token);				// parsin_res == <numero di tasti premuti>
		if (parsing_res == NULL) return 0;
		num_tasti = atoi(parsing_res);

		try { tasti = new int[num_tasti]; }
		catch (...) { return 0; }

		for (int i = 0; i < num_tasti; i++) {
			
			parsing_res = strtok_s(NULL, " ", &next_token);
			if (parsing_res == NULL) return 0;

			tasti[i] = identify_key(parsing_res);

			if (tasti[i] == -1) {
				delete[](tasti);
				return 0;
			}
		}

		// Trova la finestra relativa a hwnd e passala alla funzione do_command
		auto wnd = windows.find(std::pair<HWND, DWORD>(hwnd, pid));
		if (wnd == windows.end()) {
			if(debug) MessageBox(NULL, _T("Finestra non trovata!"), _T("DEBUG"), MB_OK);
			delete[](tasti);
			return 0;
		}

		if (!do_command(wnd->second.hwnd, tasti, num_tasti)) {
			if (debug) MessageBox(NULL, _T("Immissione del comando nella finestra non riuscito!"), _T("DEBUG"), MB_OK);
			delete[](tasti);
			return 0;
		}
		
		delete[](tasti);
		return 1;

	}
	else if (strncmp(command, "ncommands", 9) == 0) {
		
		// Tokenizza ed esegue il comando
		HWND hwnd;
		char* window_handle;
		DWORD pid;

		unsigned int num_apps;
		unsigned int num_tasti;
		int* tasti;

		char* next_token = NULL;
		char* parsing_res = strtok_s(command, " ", &next_token);	// parsing_res == "ncommands"
		if (parsing_res == NULL) return 0;

		parsing_res = strtok_s(NULL, " ", &next_token);			// parsing_res == "<numero elementi nella lista>"
		if (parsing_res == NULL) return 0;
		num_apps = atoi(parsing_res);

		for (int i = 0; i < num_apps; i++) {

			char* parsing_res = strtok_s(NULL, " ", &next_token);	// parsing_res == "command"
			if (parsing_res == NULL) return 0;

			window_handle = strtok_s(NULL, " ", &next_token);			// window_handle == "<window handle>"
			if (window_handle == NULL) return 0;
			hwnd = (HWND)atoi(window_handle);

			parsing_res = strtok_s(NULL, " ", &next_token);				// parsing_res = "<process id>"
			if (parsing_res == NULL) return 0;
			pid = (DWORD)atoi(parsing_res);

			parsing_res = strtok_s(NULL, " ", &next_token);				// parsin_res == <numero di tasti premuti>
			if (parsing_res == NULL) return 0;
			num_tasti = atoi(parsing_res);

			// Creazione della struttura dati per i tasti

			try { tasti = new int[num_tasti]; }
			catch (...) { return 0; }

			// Identificazione dei tasti
			bool skip_this = false;
			for (int j = 0; j < num_tasti; j++) {

				parsing_res = strtok_s(NULL, " ", &next_token);
				if (parsing_res == NULL) return 0;

				tasti[j] = identify_key(parsing_res);

				if (tasti[j] == -1) {
					skip_this = true;
					break;
				}
			}
			
			if (skip_this) continue;	// Passa al nuovo comando nella lista perché non è stato possibile identificare uno dei tasti

			// Trova la finestra relativa a hwnd e passala alla funzione do_command
			auto wnd = windows.find(std::pair<HWND, DWORD>(hwnd, pid));
			if (wnd == windows.end()) {
				if (debug) MessageBox(NULL, _T("Finestra non trovata!"), _T("DEBUG"), MB_OK);
				delete[](tasti);
				continue;
			}

			if (!do_command(wnd->second.hwnd, tasti, num_tasti)) {
				if (debug) MessageBox(NULL, _T("Immissione del comando nella finestra non riuscito!"), _T("DEBUG"), MB_OK);
				delete[](tasti);
				continue;
			}

			delete[](tasti);
		}
		return 1;
	}
	else if (strncmp(command, "stop", 4) == 0) {	// Richiesta di terminazione della comunicazione
		return -1;
	}
	else {
		if(debug) MessageBox(NULL, _T("Non ho riconosciuto il messaggio mandato dal client."), _T("Messaggio sconosciuto"), MB_OK | MB_ICONEXCLAMATION);
		return 0;
	}

	return 1;
}


// Ritorna -1 se non è riuscito ad identificare il tasto!
int identify_key(char* key) {

	int virtual_key = (int) strtol(key, NULL, 16);	// Il numero letto è in esadecimale

	if (virtual_key > 0 && virtual_key <= 0xFE) 
		return virtual_key;
	else 
		return -1;
}

BOOL do_command(HWND hWnd, int* tasti, int num_tasti) {

	// HWND curr_window = GetForegroundWindow();

	// Setta la finestra in focus
	if (!SetForegroundWindow(hWnd)) return FALSE;

	// Setting the input
	INPUT ip;
	ip.type = INPUT_KEYBOARD;
	ip.ki.wScan = 0;
	ip.ki.time = 0;
	ip.ki.dwExtraInfo = 0;

	for (int i = 0; i < num_tasti; i++) {
		// Press i-key
		ip.ki.wVk = tasti[i];
		ip.ki.dwFlags = 0;	// 0 == press
		SendInput(1, &ip, sizeof(INPUT));
	}

	// VERSIONE VECCHIA
	/*
	for (int i = 0; i < num_tasti && tasti[i] < 0x30 && tasti[i] > 0x5A; i++) {
		// Release i-key
		ip.ki.wVk = tasti[i];
		ip.ki.dwFlags = 1;	// 1 == release
		SendInput(1, &ip, sizeof(INPUT));
	}
	*/

	// VERSIONE NUOVA
	for (int i = 0; i < num_tasti; i++) {
		// Release i-key
		ip.ki.wVk = tasti[i];
		ip.ki.dwFlags = KEYEVENTF_KEYUP;
		SendInput(1, &ip, sizeof(INPUT));
	}

	// SetForegroundWindow(curr_window);

	return TRUE;
}


//------------------------------------------------------------------------------------
// FUNZIONE ENUMERAZIONE DELLE FINESTRE

BOOL CALLBACK EnumWindowsProc(HWND hWnd, LPARAM lParam) {
	
	if (IsWindowVisible(hWnd))
	{
		GWindow wnd;

		// GetWindowTextA(hWnd, wnd.window_name, MAX_WIN_NAME - 1); // In questo caso salvo il nome visualizzato nella finestra
		
		// if (wnd.window_name[0] == '\0') return TRUE;

		wnd.hwnd = hWnd;
		GetWindowThreadProcessId(hWnd, &wnd.process_id);

		auto curr_win = windows.find(std::pair<HWND, DWORD>(wnd.hwnd, wnd.process_id));

		if (curr_win != windows.end()) { // Vecchia finestra ancora aperta

			curr_win->second.checked = TRUE;

		}
		else { // Nuova finestra

			// EXE & ICON
			HANDLE processHandle = NULL;
			TCHAR filename[MAX_PATH];
			SHFILEINFO shfileinfo;

			processHandle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, FALSE, wnd.process_id);
			if (processHandle != NULL)
			{
				// Salvataggio nome applicazione
				if(GetModuleFileNameExA(processHandle, NULL, wnd.window_name, MAX_WIN_NAME - 1) == 0) { // Alternativa per il nome dell'applicazione!
					// Non è stato possibile ricavare il nome dell'applicazione
					CloseHandle(processHandle);
					return TRUE;
				}
				else if (wnd.window_name[0] == '\0') {
					// Questo è un comando del prompt
					CloseHandle(processHandle);
					return TRUE;
				}

				GetWindowTextA(hWnd, wnd.on_screen_name, MAX_WIN_NAME - 1); // In questo caso salvo il nome visualizzato nella finestra (usato dall'utente Client per discriminare le applicazioni che hanno più finestre grafiche)

				// Salvataggio icona applicazione
				if (GetModuleFileNameEx(processHandle, NULL, filename, MAX_PATH - 1) == 0) wnd.iconExists = FALSE;
				// if(GetProcessImageFileName(processHandle, filename, MAX_PATH) == 0) wnd.iconExists = FALSE;	// Più efficiente di GetModuleFilenameEx
				else if (SHGetFileInfo(filename, FILE_ATTRIBUTE_NORMAL, &shfileinfo, sizeof(SHFILEINFO), SHGFI_USEFILEATTRIBUTES | SHGFI_SYSICONINDEX | SHGFI_ICON | SHGFI_LARGEICON) == 0) wnd.iconExists = FALSE;
				// else if (ExtractIconEx(filename, 0, wnd.icon, NULL, 1) == NULL) wnd.iconExists = FALSE;
				else {
					wnd.iconExists = TRUE;
					wnd.icon[0] = shfileinfo.hIcon;
				}

				CloseHandle(processHandle);
			}
			else {
				// Errore apertura processHandle
				wnd.iconExists = FALSE;
			}

			// Inserimento della nuova finestra nella struttura dati
			wnd.newWin = TRUE;
			wnd.checked = TRUE;
			windows.insert(std::pair<std::pair<HWND, DWORD>, GWindow>(std::pair<HWND, DWORD>(wnd.hwnd, wnd.process_id), std::move(wnd)));
		}
	}

	return TRUE;
}