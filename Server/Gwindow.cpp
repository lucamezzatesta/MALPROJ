#pragma once

#include "stdafx.h"
#include "GWindow.h"

char* GWindow::all_to_string(int& size) {
	
	char res[BUFFER_LEN];
	char icon_buffer[ICON_SIZE];
	OVERLAPPED ol = { 0 };

	sprintf_s(res, "%s\n%s\n%d\n%d", window_name, on_screen_name, hwnd, process_id);
	size = strlen(res);

	// Salvataggio della icona nella stringa
	if (iconExists) { 

		if (!save_icon()) {
			// Se non è riuscito a salvare l'icona in C:\MALPROJIcons
			sprintf_s(res, "%s\n%s\n%d\n%d\n-1\n", window_name, on_screen_name, hwnd, process_id);	// L'icona ha dimensione -1 -> esiste ma non può essere salvata
			size = strlen(res);
			return res;
		}

		HANDLE hFile = CreateFile(
			icon_path, 
			GENERIC_READ, 
			FILE_SHARE_READ, 
			NULL, 
			OPEN_EXISTING, 
			FILE_ATTRIBUTE_NORMAL, 
			NULL);

		if (hFile == INVALID_HANDLE_VALUE) return res;	// Non invia l'icona
		
		DWORD dwNumberOfBytesRead;
		if (ReadFile(hFile, icon_buffer, ICON_SIZE-1, &dwNumberOfBytesRead, NULL) == FALSE) {
			CloseHandle(hFile);
			return res;
		}
		
		if (dwNumberOfBytesRead > 0 && dwNumberOfBytesRead < ICON_SIZE) {

			if (strlen(res) + dwNumberOfBytesRead >= BUFFER_LEN + 10) {	// Il 10 è dovuto alla presenza di altri componenti nella stringa res da aggiungere (dimensione dell'icona)
				sprintf_s(res, "%s\n%s\n%d\n%d\n-2\n", window_name, on_screen_name, hwnd, process_id);	// L'icona ha dimensione -2 -> troppo grande per essere inviata
				size = strlen(res);
				return res;
			}
			else {
				char num_of_bytes_read[10];	// Qui al posto di 10, 5 sarebbe bastato (4 numeri + \0)

				sprintf_s(num_of_bytes_read, "\n%d\n", dwNumberOfBytesRead);

				strcat_s(res, num_of_bytes_read);
				
				size_t curr_message_size = strlen(res);

				memcpy_s(res + curr_message_size, BUFFER_LEN - curr_message_size, icon_buffer, dwNumberOfBytesRead);

				size = curr_message_size + dwNumberOfBytesRead;
			}
		}
		else if (dwNumberOfBytesRead == 0) {
			MessageBox(NULL, _T("Non è stato possibile inviare l'icona."), _T("Errore lettura icona"), MB_OK | MB_ICONERROR);
			sprintf_s(res, "%s\n%s\n%d\n%d\n-3\n", window_name, on_screen_name, hwnd, process_id);	// L'icona ha dimensione -2 -> troppo grande per essere inviata
			size = strlen(res);
			return res;
		}
		else {
			MessageBox(NULL, _T("Non è stato possibile inviare l'icona."), _T("Errore inaspettato"), MB_OK | MB_ICONERROR);
			sprintf_s(res, "%s\n%s\n%d\n%d\n-4\n", window_name, on_screen_name, hwnd, process_id);	// L'icona ha dimensione -2 -> troppo grande per essere inviata
			size = strlen(res);
			return res;
		}
		
		CloseHandle(hFile);

		if (DeleteFile(icon_path) == 0) {
			// Errore distruzione icona
			// Non ci sono azioni che possono essere svolte in caso di fallimento!
		}
	}
	else {
		// Se l'icona non esiste
		sprintf_s(res, "%s\n%s\n%d\n%d\n0\n", window_name, on_screen_name, hwnd, process_id);	// L'icona ha dimensione 0 -> non esiste
		size = strlen(res);
	}

	return res;
}

char* GWindow::id_to_string() {
	
	char res[MAX_WIN_NAME];

	sprintf_s(res, "%d %d", hwnd, process_id);

	return res;
}

bool GWindow::save_icon() {

	if (iconExists) {
		//wsprintf(icon_path, L"C:\\MALPROJIcons\\icon_%d_%d.ico", (int)hwnd, (int) process_id);
		TCHAR icon_path_string[MAX_PATH];
		wsprintf(icon_path_string, L"C:\\MALPROJIcons\\icon%d.ico", (int)hwnd);
		memcpy_s(icon_path, MAX_PATH * sizeof(TCHAR), icon_path_string, MAX_PATH * sizeof(TCHAR));
		if (SaveIcon2(icon[0], 32, icon_path)) return true;
		else if (SaveIcon2(icon[0], 24, icon_path)) return true;
		else if (SaveIcon2(icon[0], 8, icon_path)) return true;
		else if (SaveIcon2(icon[0], 4, icon_path)) return true;
		else return false;
	}
	else return false;
}

HRESULT GWindow::SaveIcon(HICON hIcon, TCHAR* path) {
	// Creazione dell'interfaccia dell'immagine
	PICTDESC desc = { sizeof(PICTDESC) };
	desc.picType = PICTYPE_ICON;
	desc.icon.hicon = hIcon;
	IPicture *pPicture = 0;
	HRESULT hr = OleCreatePictureIndirect(&desc, IID_IPicture, FALSE, (void**)&pPicture);
	if (FAILED(hr)) return hr;

	// Creazione di uno stream per il file
	IStream* pStream = 0;
	CreateStreamOnHGlobal(0, TRUE, &pStream);
	LONG cbSize = 0;
	hr = pPicture->SaveAsFile(pStream, TRUE, &cbSize);

	// Scrittura sullo stream
	if (!FAILED(hr)) {
		HGLOBAL hBuff = 0;
		GetHGlobalFromStream(pStream, &hBuff);
		void* buffer = GlobalLock(hBuff);
		HANDLE hFile = CreateFile(path, GENERIC_WRITE, 0, 0, CREATE_ALWAYS, 0, 0);
		if (!hFile) hr = HRESULT_FROM_WIN32(GetLastError());
		else {
			DWORD written = 0;
			WriteFile(hFile, buffer, cbSize, &written, 0);
			CloseHandle(hFile);
		}
		GlobalUnlock(buffer);
	}
	 
	// Cleaning
	pStream->Release();
	pPicture->Release();
	return hr;
}

//-----------------------------------------------------
// SAVE ICON 2

struct ICONDIRENTRY
{
	UCHAR nWidth;
	UCHAR nHeight;
	UCHAR nNumColorsInPalette; // 0 if no palette
	UCHAR nReserved; // should be 0
	WORD nNumColorPlanes; // 0 or 1
	WORD nBitsPerPixel;
	ULONG nDataLength; // length in bytes
	ULONG nOffset; // offset of BMP or PNG data from beginning of file
};

// Helper class to release GDI object handle when scope ends:
class CGdiHandle
{
public:
	CGdiHandle(HGDIOBJ handle) : m_handle(handle) {};
	~CGdiHandle() { DeleteObject(m_handle); };
private:
	HGDIOBJ m_handle;
};

// Save icon referenced by handle 'hIcon' as file with name 'szPath'.
// The generated ICO file has the color depth specified in 'nColorBits'.
//
bool GWindow::SaveIcon2(HICON hIcon, int nColorBits, const TCHAR* szPath)
{
	ASSERT(nColorBits == 4 || nColorBits == 8 || nColorBits == 24 || nColorBits == 32);

	if (offsetof(ICONDIRENTRY, nOffset) != 12)
	{
		return false;
	}

	CDC dc;
	dc.Attach(::GetDC(NULL)); // ensure that DC is released when function ends

							  // Open file for writing:
	CFile file;
	if (!file.Open(szPath, CFile::modeWrite | CFile::modeCreate))
	{
		return false;
	}

	// Write header:
	UCHAR icoHeader[6] = { 0, 0, 1, 0, 1, 0 }; // ICO file with 1 image
	file.Write(icoHeader, sizeof(icoHeader));

	// Get information about icon:
	ICONINFO iconInfo;
	if (!GetIconInfo(hIcon, &iconInfo)) {
		DWORD debug_error = GetLastError();
	}
	CGdiHandle handle1(iconInfo.hbmColor), handle2(iconInfo.hbmMask); // free bitmaps when function ends
	BITMAPINFO bmInfo = { 0 };
	bmInfo.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
	bmInfo.bmiHeader.biBitCount = 0;    // don't get the color table
	if (!GetDIBits(dc, iconInfo.hbmColor, 0, 0, NULL, &bmInfo, DIB_RGB_COLORS))
	{
		return false;
	}

	// Allocate size of bitmap info header plus space for color table:
	int nBmInfoSize = sizeof(BITMAPINFOHEADER);
	if (nColorBits < 24)
	{
		nBmInfoSize += sizeof(RGBQUAD) * (int)(1 << nColorBits);
	}

	CAutoVectorPtr<UCHAR> bitmapInfo;
	bitmapInfo.Allocate(nBmInfoSize);
	BITMAPINFO* pBmInfo = (BITMAPINFO*)(UCHAR*)bitmapInfo;
	memcpy(pBmInfo, &bmInfo, sizeof(BITMAPINFOHEADER));

	// Get bitmap data:
	ASSERT(bmInfo.bmiHeader.biSizeImage != 0);
	CAutoVectorPtr<UCHAR> bits;
	bits.Allocate(bmInfo.bmiHeader.biSizeImage);
	pBmInfo->bmiHeader.biBitCount = nColorBits;
	pBmInfo->bmiHeader.biCompression = BI_RGB;
	if (!GetDIBits(dc, iconInfo.hbmColor, 0, bmInfo.bmiHeader.biHeight, (UCHAR*)bits, pBmInfo, DIB_RGB_COLORS))
	{
		return false;
	}

	// Get mask data:
	BITMAPINFO maskInfo = { 0 };
	maskInfo.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
	maskInfo.bmiHeader.biBitCount = 0;  // don't get the color table     
	if (!GetDIBits(dc, iconInfo.hbmMask, 0, 0, NULL, &maskInfo, DIB_RGB_COLORS))
	{
		return false;
	}
	ASSERT(maskInfo.bmiHeader.biBitCount == 1);
	CAutoVectorPtr<UCHAR> maskBits;
	maskBits.Allocate(maskInfo.bmiHeader.biSizeImage);
	CAutoVectorPtr<UCHAR> maskInfoBytes;
	maskInfoBytes.Allocate(sizeof(BITMAPINFO) + 2 * sizeof(RGBQUAD));
	BITMAPINFO* pMaskInfo = (BITMAPINFO*)(UCHAR*)maskInfoBytes;
	memcpy(pMaskInfo, &maskInfo, sizeof(maskInfo));
	if (!GetDIBits(dc, iconInfo.hbmMask, 0, maskInfo.bmiHeader.biHeight, (UCHAR*)maskBits, pMaskInfo, DIB_RGB_COLORS))
	{
		return false;
	}

	// Write directory entry:
	ICONDIRENTRY dir;
	dir.nWidth = (UCHAR)pBmInfo->bmiHeader.biWidth;
	dir.nHeight = (UCHAR)pBmInfo->bmiHeader.biHeight;
	dir.nNumColorsInPalette = (nColorBits == 4 ? 16 : 0);
	dir.nReserved = 0;
	dir.nNumColorPlanes = 0;
	dir.nBitsPerPixel = pBmInfo->bmiHeader.biBitCount;
	dir.nDataLength = pBmInfo->bmiHeader.biSizeImage + pMaskInfo->bmiHeader.biSizeImage + nBmInfoSize;
	dir.nOffset = sizeof(dir) + sizeof(icoHeader);
	file.Write(&dir, sizeof(dir));

	// Write DIB header (including color table):
	int nBitsSize = pBmInfo->bmiHeader.biSizeImage;
	pBmInfo->bmiHeader.biHeight *= 2; // because the header is for both image and mask
	pBmInfo->bmiHeader.biCompression = 0;
	pBmInfo->bmiHeader.biSizeImage += pMaskInfo->bmiHeader.biSizeImage; // because the header is for both image and mask
	file.Write(&pBmInfo->bmiHeader, nBmInfoSize);

	// Write image data:
	file.Write((UCHAR*)bits, nBitsSize);

	// Write mask data:
	file.Write((UCHAR*)maskBits, pMaskInfo->bmiHeader.biSizeImage);

	file.Close();

	return true;
}