using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.PlayerLoop;

namespace LT_OrbitSpawner
{
    public class LT_OrbitSpawner
    {

        public void LT_SpawnInOrbit(string name, string craftURL, VesselType vesselType, CelestialBody celestialBody, int heightAltitude)
        {
            Orbit orbit = null;
            ConfigNode[] partnodes;
            ShipConstruct shipConstruct = ShipConstruction.LoadShip(craftURL);

            ConfigNode emptynode = new ConfigNode();
            Vessel emptyVessel = new Vessel();
            emptyVessel.parts = shipConstruct.parts;
            ProtoVessel emptyProto = new ProtoVessel(emptynode, null);
            emptyProto.vesselRef = emptyVessel;
            uint missionID = (uint)Guid.NewGuid().GetHashCode();
            uint launchID = HighLogic.CurrentGame.launchID++;

            foreach (Part part in shipConstruct.parts)
            {
                part.launchID = launchID;
                part.missionID = missionID;
                part.temperature = 1.0;
                part.flightID = ShipConstruction.GetUniqueFlightID(HighLogic.CurrentGame.flightState);
                emptyProto.protoPartSnapshots.Add(new ProtoPartSnapshot(part, emptyProto));
            }
            foreach (ProtoPartSnapshot part in emptyProto.protoPartSnapshots)
            {
                part.storePartRefs();
            }
            List<ConfigNode> partNodesList = new List<ConfigNode>();
            foreach (var snapShot in emptyProto.protoPartSnapshots)
            {
                ConfigNode partNode = new ConfigNode("PART");
                snapShot.Save(partNode);
                partNodesList.Add(partNode);
            }
            partnodes = partNodesList.ToArray();
            ConfigNode[] extraNodes = new ConfigNode[0];
            orbit = new Orbit(0, 0.0001, heightAltitude + celestialBody.Radius, 0, 0, 0, 0, celestialBody);
            ConfigNode protoVesselNode = ProtoVessel.CreateVesselNode(name, vesselType, orbit, 0, partnodes, extraNodes);
            foreach (var part in UnityEngine.Object.FindObjectsOfType<Part>())
            {
                if (!part.vessel)
                {
                    part.Die();
                }
            }
        }
    }
}
