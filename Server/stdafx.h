// stdafx.h : file di inclusione per file di inclusione di sistema standard
// o file di inclusione specifici del progetto utilizzati di frequente, ma
// modificati raramente
//

#pragma once

#include "targetver.h"

#define WIN32_LEAN_AND_MEAN             // Escludere gli elementi utilizzati di rado dalle intestazioni di Windows
// File di intestazione di Windows:
//#include <windows.h>
#include <afx.h>
#include <afxwin.h>
#include <atlbase.h>

// File di intestazione Runtime C
#include <stdlib.h>
#include <malloc.h>
#include <memory.h>
#include <tchar.h>
#include <iostream>

// Librerie aggiuntive
#include <shellapi.h>
#include <atomic>
#include <thread>
#include <map>

// Librerie servizio TCP
#include <stdio.h>
#include <string.h>
#include <stdarg.h>
#include <winsock2.h>
#include <WS2tcpip.h>
#include <IPHlpApi.h>

// Librerie per il Monitor delle finestre
#include <Psapi.h>
#include <OleCtl.h>
#include <string>
#include <vector>
#include <ctime>

// Librerie per il salvataggio delle finestre
#include <afx.h>
#include <afxwin.h>
#include <atlbase.h>

#pragma comment(lib, "Ws2_32.lib")