using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class BeaconController : MonoBehaviour
{
    //public TextMeshProUGUI beaconsInfoText; // UI that display the beacons info (name, major, minor, range, accuracy)

    //public bool displayBeaconInfo;

    public delegate void ClosestBeaconChanged(string beaconName);
    public static event ClosestBeaconChanged ClosestBeaconChangedEvent;


    Dictionary<string, Beacon> beaconsDict = new Dictionary<string, Beacon>();
    Beacon nearestBeacon;

    public int capacity = 10;
    Queue<Beacon> beaconsQueue; //Use to calculate beacon detection accuracy

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
        // Update beacons info dictionary
        foreach (Beacon beacon in beacons)
        {
            if (beaconsDict.ContainsKey(beacon.regionName))
            {
                beaconsDict[beacon.regionName] = beacon;
            }
            else
            {
                beaconsDict.Add(beacon.regionName, beacon);
            }
        }

        //if (displayBeaconInfo)
        //{
        //    string beaconsInfoUpdate = "";

        //    foreach (Beacon beacon in beaconsDict.Values)
        //    {
        //        beaconsInfoUpdate += "name : " + beacon.regionName + "\r\n"
        //            + "major : " + beacon.major + "\r\n"
        //            + "minor : " + beacon.minor + "\r\n"
        //            + "range : " + beacon.range.ToString() + "\r\n"
        //            + "accuracy : " + beacon.accuracy.ToString() + "\r\n\r\n";
        //    }

        //    beaconsInfoText.text = beaconsInfoUpdate;
        //}


        Beacon newNearestBeacon = beaconsDict
            .Where(b => b.Value.accuracy >= 0) // On iOS, negative accuracy means that the beacon is not detected
            .ToDictionary(i => i.Key, i => i.Value)
            .Aggregate((l, r) => l.Value.accuracy < r.Value.accuracy ? l : r).Value;

        CheckAccuracy(newNearestBeacon);

    }

    private void CheckAccuracy(Beacon detectedBeacon)
    {
        if (beaconsQueue.Count >= capacity)
        {
            Beacon removedBeacon = beaconsQueue.Dequeue();
        }

        //Add Accuracy
        beaconsQueue.Enqueue(detectedBeacon);

        //Calculate Accuracy
        Dictionary<Beacon, int> accuracies = new Dictionary<Beacon, int>();



        foreach (Beacon beacon in beaconsQueue.ToList())
        {
            if (!accuracies.ContainsKey(beacon))
                accuracies.Add(beacon, 0);

            accuracies[beacon]++;
        }

        Beacon newNearestBeacon = detectedBeacon;
        foreach (KeyValuePair<Beacon, int> beaconAccuracy in accuracies)
        {
            if (beaconAccuracy.Value > accuracies[newNearestBeacon])
                newNearestBeacon = beaconAccuracy.Key;
        }

        // If nearest beacon changed
        if (newNearestBeacon != null && (nearestBeacon == null || newNearestBeacon.regionName != nearestBeacon.regionName))
        {
            // Launch event
            ClosestBeaconChangedEvent(newNearestBeacon.regionName);
        }

        nearestBeacon = newNearestBeacon;

    }

}
