using KSPAchievements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LT_OrbitSpawner
{
    public class TestSpawner : PartModule
    {
        [KSPEvent(guiName = "Test Spawner", guiActive = true, externalToEVAOnly = false, guiActiveEditor = false, active = true, guiActiveUnfocused = true, unfocusedRange = 5.0f)]
        public void testSpawner()
        {
            LT_SpawnInOrbit("test vessel", "GameData\\000_LoonToolbox\\TestShip\\Aeris3A.craft", VesselType.Ship, FlightGlobals.fetch.bodies.ElementAt(1), 100000);
        }

        public void LT_SpawnInOrbit(string name, string craftURL, VesselType vesselType, CelestialBody celestialBody, int heightAltitude)
        {
            ProtoVessel CPVProtoBackup = null;
            Orbit orbit = null;
            //ShipConstruct shipConstruct = ShipConstruction.LoadShip(craftURL);
            orbit = new Orbit(0, 0.0001, heightAltitude + celestialBody.Radius, 0, 0, 0, 0, celestialBody);
            ConfigNode craftConfigNode = ConfigNode.Load(craftURL);
            ScreenMessages.PostScreenMessage("Setting Orbit");
            var type = vesselType;
            
            if (craftConfigNode == null) 
            {
                ScreenMessages.PostScreenMessage("Vessel Invalid!");
                return;
            }
            var ShipConstructError = string.Empty;
            if (!ShipConstruction.AllPartsFound(craftConfigNode, ref ShipConstructError))
            {
                ScreenMessages.PostScreenMessage("Parts in Vessel are unavailable!");
                return;
            }
            var emptyProto = CreateProtoVessel();
            if (emptyProto == null)
            {
                ScreenMessages.PostScreenMessage("Vessel Invalid!");
                return;
            }
            CPVProtoBackup = emptyProto;
            var morepartnodes = CPVProtoBackup.protoPartSnapshots.Select(s =>
            {
                ConfigNode node = new ConfigNode("PART");
                s.Save(node);
                return node;
            })
                .ToArray();
            ScreenMessages.PostScreenMessage("Creating Snapshots");
            var vesselConfigNode = ProtoVessel.CreateVesselNode(name, type, orbit, 0, morepartnodes, craftConfigNode);
            ScreenMessages.PostScreenMessage("Passing Checks...");
            
            var spawnedProtoVessel = new ProtoVessel(vesselConfigNode, HighLogic.CurrentGame);
            var spawnedVessel = FlightGlobals.Vessels.Last();
            spawnedVessel.protoVessel.stage = int.MaxValue;
            FlightGlobals.SetActiveVessel(spawnedVessel);
            ScreenMessages.PostScreenMessage("Spawning!");
            ProtoVessel CreateProtoVessel()
            {
                Vessel CPVVessel = null;
                ProtoVessel CPVProtoVessel = null;
                ShipConstruct CPVConstruct = null;
                
                try 
                {
                    var CPVConstructBackup = ShipConstruction.ShipConfig;
                    CPVConstruct = ShipConstruction.LoadShip(craftURL);
                    ShipConstruction.ShipConfig = CPVConstructBackup;
                    CPVVessel = new GameObject().AddComponent<Vessel>();
                    CPVVessel.parts = CPVConstruct.parts;
                    CPVProtoVessel = new ProtoVessel(new ConfigNode(), null)
                    {
                        vesselName = CPVConstruct.shipName,
                        vesselRef = CPVVessel
                    };
                    var missionID = (uint)Guid.NewGuid().GetHashCode();
                    var launchID = HighLogic.CurrentGame.launchID++;
                    var rootPart = CPVConstruct.First();

                    foreach (Part part in CPVConstruct.parts)
                    {
                        part.launchID = launchID;
                        part.missionID = missionID;
                        part.temperature = 1.0;
                        part.flightID = ShipConstruction.GetUniqueFlightID(HighLogic.CurrentGame.flightState);
                        part.flagURL = HighLogic.CurrentGame.flagURL;
                        part.UpdateOrgPosAndRot(rootPart);
                        part.vessel = CPVVessel;
                        var partSnapshot = new ProtoPartSnapshot(part, CPVProtoVessel);
                        CPVProtoVessel.protoPartSnapshots.Add(partSnapshot);
                    }
                    foreach (ProtoPartSnapshot part in CPVProtoVessel.protoPartSnapshots)
                    {
                        part.storePartRefs();
                    }
                    CPVProtoBackup = CPVProtoVessel;
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
                finally 
                { 
                    if (CPVConstruct != null && CPVConstruct.parts != null && CPVConstruct.parts.Count > 0)
                    {
                        foreach (var part in CPVConstruct.parts)
                        {
                            Destroy(part.gameObject);
                        }
                    }
                    if (vessel != null)
                    {
                        Destroy(vessel.gameObject);
                    }
                    
                }
                return CPVProtoVessel;
            }
        }
    }
}
