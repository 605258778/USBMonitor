#include <windows.h>
#include <dbt.h>
#include <stdio.h>
#include <math.h>
#define LOCK_TIMEOUT        10000       // 10 Seconds
#define LOCK_RETRIES        20
/***************
Function：	UninstallUsb
Description：	根据盘符将usb设备弹出
Parameters：
discId	要弹出的盘符
Returns：
-1	失败
0	成功
***************/
static int UninstallUsb(char *discId)
{
	DWORD accessMode = GENERIC_WRITE | GENERIC_READ;
	DWORD shareMode = FILE_SHARE_READ | FILE_SHARE_WRITE;
	HANDLE hDevice;
	long	bResult = 0;
	DWORD retu = 0;
	DWORD dwError;
	DWORD dwBytesReturned;
	DWORD dwSleepAmount;
	int nTryCount;
	char szDriv[10];
	if (discId == NULL) {
		return 0;
	}
	dwSleepAmount = LOCK_TIMEOUT / LOCK_RETRIES;
	sprintf(szDriv, "\\\\.\\%s:", discId);
	hDevice = CreateFile(szDriv, accessMode, shareMode, NULL, OPEN_EXISTING, 0, NULL);
	if (hDevice == INVALID_HANDLE_VALUE) {
		printf("uninstallusb createfile failed error:%d\n", GetLastError());
		return -1;
	}
#if 0
	//此循环是用于锁定要弹出的U盘设备，如果U盘在使用，则循环等待
	// Do this in a loop until a timeout period has expired
	for (nTryCount = 0; nTryCount < LOCK_RETRIES; nTryCount++) {
		if (DeviceIoControl(hDevice, FSCTL_LOCK_VOLUME, NULL, 0, NULL, 0, &dwBytesReturned, NULL)) {
			break;
		}
	}
	//卸载U盘卷，不论是否在使用
	dwBytesReturned = 0;
	if (!DeviceIoControl(hDevice, FSCTL_DISMOUNT_VOLUME, NULL, 0, NULL, 0, &dwBytesReturned, NULL)) {
		printf("deviceIoConrol FSCTL_DISMOUNT_VOLUME failed\n");
	}
#endif
	dwBytesReturned = 0;
	PREVENT_MEDIA_REMOVAL PMRBuffer;
	PMRBuffer.PreventMediaRemoval = FALSE;
	if (!DeviceIoControl(hDevice, IOCTL_STORAGE_MEDIA_REMOVAL, &PMRBuffer, sizeof(PREVENT_MEDIA_REMOVAL), NULL, 0, &dwBytesReturned, NULL)) {
		printf("DeviceIoControl IOCTL_STORAGE_MEDIA_REMOVAL failed:%d\n", GetLastError());
	}
	bResult = DeviceIoControl(hDevice, IOCTL_STORAGE_EJECT_MEDIA, NULL, 0, NULL, 0, &retu, NULL);
	if (!bResult) {
		CloseHandle(hDevice);
		printf("uninstallusb DeviceIoControl failed error:%d\n", GetLastError());
		return -1;
	}
	CloseHandle(hDevice);
	return 0;
}
int main()
{
	//UninstallUsb("H");
	return 0;
}