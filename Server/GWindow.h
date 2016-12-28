#pragma once

#include "stdafx.h"

#define MAX_WIN_NAME 255
#define BUFFER_LEN 8192
#define ICON_SIZE 8192

class GWindow {

	HRESULT SaveIcon(HICON hIcon, TCHAR* path);
	bool SaveIcon2(HICON hIcon, int nColorBits, const TCHAR* szPath);

public:

	char	window_name[MAX_WIN_NAME];
	char	on_screen_name[MAX_WIN_NAME];
	HWND	hwnd;
	DWORD	process_id = 0;
	BOOL	newWin;
	BOOL	checked;
	BOOL	iconExists = FALSE;
	HICON	icon[1];
	TCHAR	icon_path[MAX_PATH];

	DWORD g_BytesTransferred = 0;

	char*	all_to_string(int& size);	// Trasforma la struttura dati in una stringa con il seguente formato:
											//	<window_name>
											//	<window handle>
											//	<process_id>
											//  <icona> (facoltativo)

	char*	id_to_string();		// Trasforma la struttura dati in una stringa con il seguente formato:
									// <window_name> <process_id>

	bool	save_icon();
	
};