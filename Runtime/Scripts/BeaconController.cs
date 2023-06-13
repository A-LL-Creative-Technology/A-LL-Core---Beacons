using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Android;

public class BeaconController : MonoBehaviour
{
    //public TextMeshProUGUI beaconsInfoText; // UI that display the beacons info (name, major, minor, range, accuracy)

    //public bool displayBeaconInfo;

    public delegate void ClosestBeaconChanged(string beaconName);
    public static event ClosestBeaconChanged ClosestBeaconChangedEvent;


    Dictionary<string, Beacon> beaconsDict = new Dictionary<string, Beacon>();
    Beacon nearestBeacon;

    public int capacity = 11;
    Queue<Beacon> beaconsQueue; //Use to calculate beacon detection accuracy

    private void Awake()
    {
#if UNITY_2020_2_OR_NEWER
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation)
          || !Permission.HasUserAuthorizedPermission(Permission.FineLocation)
          || !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_SCAN")
          //|| !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_ADVERTISE")
          || !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_CONNECT"))
            Permission.RequestUserPermissions(new string[] {
            Permission.CoarseLocation,
            Permission.FineLocation,
            "android.permission.BLUETOOTH_SCAN",
            //"android.permission.BLUETOOTH_ADVERTISE",
            "android.permission.BLUETOOTH_CONNECT"
        });
#endif
#endif
    }
    
    private void Start()
    {
        beaconsQueue = new Queue<Beacon>(capacity);
    }

    /// <summary>
    /// Enable Bluetooth and starts looking for beacons
    /// </summary>
    public void StartScanning()
    {
        try
        {
            iBeaconReceiver.BeaconRangeChangedEvent += BeaconDetected;
            BluetoothState.BluetoothStateChangedEvent += BlueToothStateChanged;
            BluetoothState.EnableBluetooth();
        }
        catch (iBeaconException e)
        {
            //TODO Catch exception
            Debug.LogError(e);
        }
    }

    /// <summary>
    /// Stops detecting beacons
    /// </summary>
    public void StopScanning()
    {
        iBeaconReceiver.BeaconRangeChangedEvent -= BeaconDetected;
        BluetoothState.BluetoothStateChangedEvent -= BlueToothStateChanged;
        iBeaconReceiver.Stop();
    }

    private void BlueToothStateChanged(BluetoothLowEnergyState state)
    {
        Debug.Log("Current BLE state is: " + state.ToString());

        // Wait for bluetooth to be powered on before scan (iOS won't work eitherway)
        if (state == BluetoothLowEnergyState.POWERED_ON) iBeaconReceiver.Scan();
    }

    private void BeaconDetected(Beacon[] beacons)
    {
        if (beacons.Length == 0)
            return; //No beacons detected

        Beacon newNearestBeacon = beacons[0];

        foreach (Beacon beacon in beacons)
        {
            if (beacon.accuracy < 0)
                continue; //On iOS, negative accuracy means that the beacon is not detected

            if (beacon.accuracy < newNearestBeacon.accuracy)
                newNearestBeacon = beacon;
        }

        if (newNearestBeacon.accuracy < 0)
            return; //On iOS, negative accuracy means that the beacon is not detected

        if (nearestBeacon == null || (newNearestBeacon.accuracy < nearestBeacon.accuracy && !newNearestBeacon.regionName.Equals(nearestBeacon.regionName)) || nearestBeacon.accuracy < 0)
        {
            //New beacon detected
            ClosestBeaconChangedEvent(newNearestBeacon.regionName);

            nearestBeacon = newNearestBeacon;
        }

        if (newNearestBeacon.regionName.Equals(nearestBeacon.regionName)) //Same beacon changed distance
            nearestBeacon = newNearestBeacon;
    }


}
