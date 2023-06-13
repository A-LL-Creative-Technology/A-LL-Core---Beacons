# Augmented Reality Core package of A-LL Creative Technology mobile Unity Framework

## Installation

To use this package in a unity project :

1. Clone the repository in local directory.
2. In Unity, open Window > Package Manager and "Add Package from git url ..." and insert this URL https://laurent-all@bitbucket.org/a-lltech/a-ll-core-ar.git.
3. Add the following third-party packages from the Package Manager
    1. Connect using the lab@a-ll.tech account in Unity
    2. Select "My Assets" in the Package Manager to display paid Packages from the Asset Store
    3. Select and import all these packages
        - iBeacon

## Adding permissions to the manifest
The specific runtime permissions you request depend on the Android SDK version you are targeting (the “targetSdkVeion” in build.gradle”) and the version of Android on which your app runs. If

You must manually add this permission to the ApplicationManifest.xml:

`<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />`

If you target Android 12 or higher (SDK 31+) you must also request:

```
<uses-permission android:name="android.permission.BLUETOOTH_SCAN"/>

<!-- Below is only needed if you want to read the device name or establish a bluetooth connection -->
<uses-permission android:name="android.permission.BLUETOOTH_CONNECT"/>

<!-- Below is only needed if you want to emit beacon transmissions -->
<uses-permission android:name="android.permission.BLUETOOTH_ADVERTISE/> 
```

If you want to detect beacons in the background, you must also add:

`<uses-permission android:name="android.permission.ACCESS_BACKGROUND_LOCATION" />`

## Requesting permissions at runtime
On Android, you must add code to request permission at runtime. Here is an example of code:

```
using UnityEngine.Android;
...

#if UNITY_2020_2_OR_NEWER
#if UNITY_ANDROID
if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation)
  || !Permission.HasUserAuthorizedPermission(Permission.FineLocation)
  || !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_SCAN")
  || !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_ADVERTISE")
  || !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_CONNECT"))
  Permission.RequestUserPermissions(new string[] {
    Permission.CoarseLocation,
    Permission.FineLocation,
    "android.permission.BLUETOOTH_SCAN",
    "android.permission.BLUETOOTH_ADVERTISE",
    "android.permission.BLUETOOTH_CONNECT"
  });
#endif
#endif
```