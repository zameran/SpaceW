namespace Experimental
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using UnityEngine;

    [SelectionBase]
    public class Vessel : MonoBehaviour
    {
        public enum State
        {
            INACTIVE,
            ACTIVE,
            DEAD
        }

        public enum Situations
        {
            LANDED = 1,
            SPLASHED,
            PRELAUNCH = 4,
            FLYING = 8,
            SUB_ORBITAL = 16,
            ORBITING = 32,
            ESCAPING = 64,
            DOCKED = 128
        }

        public Rigidbody rb;

        private class CenterVectorHelper
        {
            public Vector3 center;

            public float c;

            public CenterVectorHelper()
            {
                center = Vector3.zero;
                c = 0f;
            }

            public void Clear()
            {
                center.x = 0f;
                center.y = 0f;
                center.z = 0f;
                c = 0f;
            }
        }

        private const float maxVisibleDistanceSqr = 2.5E+07f;

        public Guid id;

        public string vesselName;

        //public List<VesselModule> vesselModules;

        //public List<Part> parts;

        //public Part rootPart;

        //[NonSerialized]
        //private Part referenceTransformPart;

        //[NonSerialized]
        //private Part referenceTransformPartRecall;

        public uint referenceTransformId;

        public uint referenceTransformIdRecall;

        public OrbitDriver orbitDriver;

        //public PatchedConicSolver patchedConicSolver;

        //public PatchedConicRenderer patchedConicRenderer;

        //public OrbitTargeter orbitTargeter;

        //public OrbitRenderer orbitRenderer;

        //public MapObject mapObject;

        //public FlightCtrlState ctrlState;

        public int currentStage;

        public Quaternion srfRelRotation;

        public double longitude;

        public double latitude;

        public double altitude;

        //public ProtoVessel protoVessel;

        public bool loaded;

        public Vessel.State state;

        public bool packed;

        public bool Landed;

        public bool Splashed;

        public string landedAt;

        public string landedAtLast;

        private float camhdg;

        private float campth;

        private float camMode = -1f;

        //private bool persistent;

        public Situations situation;

        public double missionTime;

        public double launchTime;

        public Vector3d obt_velocity;

        public Vector3d srf_velocity;

        public Vector3 rb_velocity;

        public double obt_speed;

        public Vector3d acceleration;

        private Vector3d[] accelSmoothing;

        private int accelSmoothingCursor;

        public Vector3d perturbation;

        public double specificAcceleration;

        public Vector3d upAxis;

        public Vector3 CoM;

        public Vector3 MOI;

        public Vector3 angularVelocity = Vector3.zero;

        public Vector3 angularMomentum = Vector3.zero;

        public bool handlePhysicsStats = true;

        public double geeForce;

        public double geeForce_immediate;

        public Vector3d gForce;

        public Vector3d CentrifugalAcc;

        public Vector3d CoriolisAcc;

        public double verticalSpeed;

        public double horizontalSrfSpeed;

        public double srfSpeed;

        public double indicatedAirSpeed;

        public double mach;

        public double speed;

        public double externalTemperature;

        public double atmosphericTemperature;

        public double staticPressurekPa;

        public double dynamicPressurekPa;

        public double atmDensity;

        public double speedOfSound;

        public double distanceToSun;

        public Vector3d up;

        public Vector3d north;

        public Vector3d east;

        public double convectiveMachFlux;

        public double convectiveCoefficient;

        public double solarFlux;

        public bool directSunlight;

        public double totalMass;

        public double lastUT = -1.0;

        public double waterOffset;

        private Vector3d lastVel;

        private Vector3d nextVel;

        //public VesselType vesselType;

        //private VesselValues vesselValues;

        private Vector3 rootOrgPos;

        private Quaternion rootOrgRot;

        private bool isControllable;

        //public VesselRanges vesselRanges;

        private bool physicsHoldLock;

        private int framesAtStartup;

        private RaycastHit hit;

        public float heightFromTerrain = -1f;

        public float heightFromSurface = -1f;

        public double terrainAltitude;

        public double pqsAltitude;

        public Vector3 terrainNormal;

        public GameObject objectUnderVessel;

        private double FG_geeForce;

        public Transform vesselTransform;

        private Vessel.Situations lastSituation;

        private bool noControlLockSet;

        private Vessel.CenterVectorHelper centerHelper = new Vessel.CenterVectorHelper();

        public Vector3 localCoM;

        private bool autoClean;

        private string autoCleanReason = string.Empty;

        //public List<Part> Parts
        //{
        //    get
        //    {
        //        return this.parts;
        //    }
        //}

        public Transform ReferenceTransform
        {
            get
            {
                return this.vesselTransform;
            }
        }

        public Orbit orbit
        {
            get
            {
                return orbitDriver.orbit;
            }
        }

        public CelestialBody mainBody
        {
            get
            {
                return orbit.referenceBody;
            }
        }

        public bool LandedOrSplashed
        {
            get
            {
                return Landed || Splashed;
            }
        }

        public bool HoldPhysics
        {
            get
            {
                return physicsHoldLock;
            }
        }

        //public bool isCommandable
        //{
        //    get
        //    {
        //        return vesselType > VesselType.Debris;
        //    }
        //}

        public bool isActiveVessel
        {
            get
            {
                return FlightGlobals.ActiveVessel == this;
            }
        }

        public bool IsRecoverable
        {
            get
            {
                return this.LandedOrSplashed && this.mainBody.isHomeWorld;
            }
        }

        public Vector3 CurrentCoM
        {
            get
            {
                return (!this.packed) ? ((Vector3d)this.CoM + (Vector3d)this.rb_velocity * Time.fixedDeltaTime) : ((Vector3d)this.CoM - this.obt_velocity * (double)Time.fixedDeltaTime);
            }
        }

        public bool IsControllable
        {
            get
            {
                return this.isControllable;
            }
        }

        public bool PatchedConicsAttached
        {
            get;
            private set;
        }

        public bool AutoClean
        {
            get
            {
                return this.autoClean;
            }
        }

        public string AutoCleanReason
        {
            get
            {
                return this.autoCleanReason;
            }
        }

        public Vector3d GetWorldPos3D()
        {
            return (!this.LandedOrSplashed) ? (this.orbit.referenceBody.position + this.orbit.pos.xzy) : this.orbit.referenceBody.GetWorldSurfacePosition(this.latitude, this.longitude, this.altitude);
        }

        private void Awake()
        {
            this.vesselTransform = base.transform;
            this.framesAtStartup = Time.frameCount;
            this.accelSmoothing = new Vector3d[20];
            this.accelSmoothingCursor = 0;
            //this.actionGroups = new ActionGroupList(this);
            //this.ctrlState = new FlightCtrlState();
            //this.vesselValues = new VesselValues(this);
            //this.autopilot = new VesselAutopilot(this);
            //this.discoveryInfo = new DiscoveryInfo(this);
            //this.flightPlanNode = new ConfigNode("FLIGHTPLAN");
        }

        private void AddPhysicsHoldLock()
        {
            //InputLockManager.SetControlLock(~(ControlTypes.UI | ControlTypes.PAUSE | ControlTypes.CAMERACONTROLS | ControlTypes.QUICKLOAD | ControlTypes.GUI | ControlTypes.MAP_UI | ControlTypes.MAP_TOGGLE), "physicsHold");
            this.physicsHoldLock = true;
        }

        private void RemovePhysicsHoldLock()
        {
            if (this.physicsHoldLock)
            {
                //InputLockManager.RemoveControlLock("physicsHold");
                this.physicsHoldLock = false;
            }
        }

        private bool PhysicsHoldExpired()
        {
            int num = (!this.Landed) ? 25 : 75;
            return Time.frameCount - this.framesAtStartup >= num;
        }

        public bool Initialize(bool fromShipAssembly = false)
        {
            //this.rootPart = base.GetComponent<Part>();
            //this.evaController = base.GetComponent<KerbalEVA>();
            //this.rootOrgPos = this.rootPart.orgPos;
            //this.rootOrgRot = this.rootPart.orgRot;
            //this.parts = new List<Part>();
            //this.findVesselParts(this.rootPart, !fromShipAssembly);
            //int count = this.parts.get_Count();
            //for (int i = 0; i < count; i++)
            //{
            //    this.parts.get_Item(i).InitializeModules();
            //}
            if (this.vesselTransform == null)
            {
                Debug.LogWarning("[Vessel]: " + base.name + " failed to initialize! Transform is null!", base.gameObject);
                return false;
            }
            this.orbitDriver = (base.GetComponent<OrbitDriver>() ?? base.gameObject.AddComponent<OrbitDriver>());
            this.orbitDriver.referenceBody = FlightGlobals.getMainBody(this.vesselTransform.position);
            //this.vesselType = this.FindDefaultVesselType();
            //if (this.vesselType > VesselType.Debris)
            //{
            //    for (int j = 0; j < count; j++)
            //    {
            //        Part part = this.parts.get_Item(j);
            //        part.isConnected = true;
            //        part.isAttached = true;
            //    }
            //}
            this.orbitDriver.updateMode = ((!fromShipAssembly) ? OrbitDriver.UpdateMode.TRACK_Phys : OrbitDriver.UpdateMode.IDLE);
            //this.AddOrbitRenderer();
            //this.vesselModules = VesselModuleManager.AddModulesToVessel(this);
            FlightGlobals.Vessels.Add(this);
            this.launchTime = Planetarium.GetUniversalTime();
            this.missionTime = 0.0;
            this.loaded = true;
            //this.persistent = this.isPersistent;
            this.MakeInactive();
            if (fromShipAssembly)
            {
                this.latitude = this.mainBody.GetLatitude(this.vesselTransform.position);
                this.longitude = this.mainBody.GetLongitude(this.vesselTransform.position);
                this.altitude = this.mainBody.GetAltitude(this.vesselTransform.position);
                this.srfRelRotation = Quaternion.Inverse(this.mainBody.bodyTransform.rotation) * this.vesselTransform.rotation;
                this.upAxis = FlightGlobals.getUpAxis(this.vesselTransform.position);
                this.heightFromTerrain = Vector3.Dot(this.vesselTransform.position, this.upAxis);
                this.terrainNormal = this.vesselTransform.InverseTransformDirection(this.upAxis);
                this.situation = (this.lastSituation = Vessel.Situations.PRELAUNCH);
                this.packed = true;
                MonoBehaviour.print("[" + this.vesselName + "]: Ready to Launch - waiting to start physics...");
                this.AddPhysicsHoldLock();
                //GameEvents.onVesselCreate.Fire(this);
            }
            else
            {
                this.packed = false;
                this.lastUT = Planetarium.GetUniversalTime();
                //this.FallBackReferenceTransform();
                this.updateSituation();
                //GameEvents.onVesselCreate.Fire(this);
                //GameEvents.onVesselWasModified.Fire(this);
            }
            //this.protoVessel = new ProtoVessel(this);
            return true;
        }

        public void MakeInactive()
        {
            if (this.noControlLockSet)
            {
                //InputLockManager.RemoveControlLock("vessel_noControl_" + this.id.ToString());
                this.noControlLockSet = false;
            }
            if (this.state == Vessel.State.DEAD)
            {
                return;
            }
            if (this.state == Vessel.State.ACTIVE)
            {
                //this.camhdg = FlightCamera.CamHdg;
                //this.campth = FlightCamera.CamPitch;
                //this.camMode = (float)FlightCamera.CamMode;
            }
            this.state = Vessel.State.INACTIVE;
            //int count = this.parts.get_Count();
            //for (int i = 0; i < count; i++)
            //{
            //    Part part = this.parts.get_Item(i);
            //    part.SendMessage("OnVesselInactive", SendMessageOptions.DontRequireReceiver);
            //}
            base.SendMessage("OnVesselInactive", SendMessageOptions.DontRequireReceiver);
            //this.DetachPatchedConicsSolver();
            //this.DespawnCrew();
        }

        public void GoOnRails()
        {
            if (this.packed)
            {
                return;
            }
            //GameEvents.onVesselGoOnRails.Fire(this);
            //this.ctrlState.Neutralize();
            //MonoBehaviour.print("Packing " + this.vesselName + " for orbit");
            if (this.loaded)
            {
                //int count = this.parts.get_Count();
                //for (int i = 0; i < count; i++)
                //{
                //    Part part = this.parts.get_Item(i);
                //    part.Pack();
                //}
                //if (this.Landed)
                //{
                //    this.GetHeightFromTerrain();
                //}
            }
            this.packed = true;
            this.orbitDriver.SetOrbitMode(OrbitDriver.UpdateMode.UPDATE);
            base.SendMessage("OnVesselPack", SendMessageOptions.DontRequireReceiver);
        }

        public void GoOffRails()
        {
            if (!this.packed) return;
            if (!this.PhysicsHoldExpired())
            {
                return;
            }

            this.RemovePhysicsHoldLock();

            this.packed = false;
            //int count = this.parts.get_Count();
            //for (int i = 0; i < count; i++)
            //{
            //    this.parts.get_Item(i).Unpack();
            //    this.parts.get_Item(i).InitializeModules();
            //}
            this.orbitDriver.SetOrbitMode(OrbitDriver.UpdateMode.TRACK_Phys);
            //for (int j = 0; j < count; j++)
            //{
            //    this.parts.get_Item(j).ResumeVelocity();
            //}
            for (int k = 0; k < this.accelSmoothing.Length; k++)
            {
                this.accelSmoothing[k].Zero();
            }
            //if (CheatOptions.PauseOnVesselUnpack)
            //{
            //    Debug.Break();
            //}
            if (FlightGlobals.ActiveVessel == this)
            {
                if (this.situation == Vessel.Situations.PRELAUNCH)
                {
                    //FlightInputHandler.SetLaunchCtrlState();
                }
                else
                {
                    //FlightInputHandler.ResumeVesselCtrlState(this);
                }
            }
            base.SendMessage("OnVesselUnpack", SendMessageOptions.DontRequireReceiver);
            //GameEvents.onVesselGoOffRails.Fire(this);
        }

        public void CalculatePhysicsStats()
        {
            this.totalMass = (double)this.GetTotalMass();
            this.CoM = this.findWorldCenterOfMass();
            if (this.rb != null)
            {
                this.rb_velocity = this.rb.GetPointVelocity(this.CoM);
                this.angularVelocity = Quaternion.Inverse(this.ReferenceTransform.rotation) * this.rb.angularVelocity;
            }
            else
            {
                this.rb_velocity = Vector3.zero;
                this.angularVelocity = Vector3.zero;
            }
            this.MOI = this.findLocalMOI(this.CoM);
            this.angularMomentum.x = this.MOI.x * this.angularVelocity.x;
            this.angularMomentum.y = this.MOI.y * this.angularVelocity.y;
            this.angularMomentum.z = this.MOI.z * this.angularVelocity.z;
        }

        private void FixedUpdate()
        {
            if (this.state == Vessel.State.DEAD)
            {
                return;
            }
            if (this.LandedOrSplashed && this.packed)
            {
                this.SetRotation(this.mainBody.bodyTransform.rotation * this.srfRelRotation);
                this.SetPosition(this.mainBody.GetWorldSurfacePosition(this.latitude, this.longitude, this.altitude));
            }
            else
            {
                this.latitude = this.mainBody.GetLatitude(this.vesselTransform.position);
                this.longitude = this.mainBody.GetLongitude(this.vesselTransform.position);
                this.altitude = this.mainBody.GetAltitude(this.vesselTransform.position);
                this.srfRelRotation = Quaternion.Inverse(this.mainBody.bodyTransform.rotation) * this.vesselTransform.rotation;
            }
            //if (this.loaded && !this.packed)
            //{
            //    this.GetHeightFromTerrain();
            //}
            if (!this.LandedOrSplashed) //else if
            {
                this.heightFromTerrain = -1f;
            }
            if (this.heightFromTerrain != -1f)
            {
                this.terrainAltitude = this.altitude - (double)this.heightFromTerrain;
                this.pqsAltitude = this.terrainAltitude;
            }
            else
            {
                //this.pqsAltitude = this.PQSAltitude();
                this.terrainAltitude = this.pqsAltitude;
            }

            if (!this.LandedOrSplashed && this.altitude < ((this.terrainAltitude == -1.0) ? ((!this.loaded) ? -0.1 : -250.0) : this.terrainAltitude))
            {
                if (this.loaded)
                {
                    //GameEvents.onCrash.Fire(new EventReport(FlightEvents.CRASH, this.rootPart, this.vesselName, this.mainBody.theName, 0, string.Empty, 0f));
                    //this.rootPart.explode();
                }
                else
                {
                    //this.MurderCrew();
                    //this.Die();
                }
                return;
            }

            this.obt_velocity = this.orbit.GetRelativeVel();
            this.obt_speed = this.obt_velocity.magnitude;
            this.srf_velocity = this.orbit.GetVel() - this.mainBody.getRFrmVel(this.vesselTransform.position);
            if (this.handlePhysicsStats)
            {
                this.CalculatePhysicsStats();
            }
            else
            {
                this.handlePhysicsStats = true;
            }
            this.upAxis = FlightGlobals.getUpAxis(this.mainBody, this.CoM);
            this.verticalSpeed = Vector3d.Dot(this.obt_velocity, this.upAxis);
            double sqrMagnitude = this.srf_velocity.sqrMagnitude;
            if (sqrMagnitude > 0.0)
            {
                this.srfSpeed = Math.Sqrt(sqrMagnitude);
                double num = sqrMagnitude - this.verticalSpeed * this.verticalSpeed;
                if (num <= 0.0 || double.IsNaN(num))
                {
                    this.horizontalSrfSpeed = 0.0;
                }
                else
                {
                    this.horizontalSrfSpeed = Math.Sqrt(num);
                }
            }
            else
            {
                this.srfSpeed = 0.0;
                this.horizontalSrfSpeed = 0.0;
            }
            this.up = ((Vector3d)this.CoM - this.mainBody.position).normalized;
            //this.north = Vector3d.Exclude(this.up, this.mainBody.position + this.mainBody.transform.up * (float)this.mainBody.Radius - this.CoM).normalized;
            this.east = this.mainBody.getRFrmVel(this.CoM).normalized;
            if (this.loaded && !this.packed)
            {
                this.accelSmoothing[this.accelSmoothingCursor++] = (this.obt_velocity - this.lastVel) / (double)Time.fixedDeltaTime;
                this.accelSmoothingCursor %= this.accelSmoothing.Length;
                this.acceleration.Zero();
                for (int i = 0; i < this.accelSmoothing.Length; i++)
                {
                    this.acceleration += this.accelSmoothing[i];
                }
                this.acceleration /= (double)this.accelSmoothing.Length;
                this.perturbation = this.acceleration - this.gForce - this.CentrifugalAcc - this.CoriolisAcc * 0.5;
                this.geeForce = this.perturbation.magnitude / 9.81;
                if (this.state == Vessel.State.ACTIVE)// && this.ctrlState.mainThrottle > 0f)
                {
                    //this.specificAcceleration = this.geeForce * 9.81 / (double)this.ctrlState.mainThrottle;
                    this.specificAcceleration = this.geeForce * 9.81;
                }
                this.geeForce_immediate = ((this.obt_velocity - this.lastVel) / (double)Time.fixedDeltaTime - this.gForce - this.CentrifugalAcc - this.CoriolisAcc * 0.5).magnitude / 9.81;
                Debug.DrawRay(this.CoM, this.perturbation * 10.0, Color.yellow);
            }
            else
            {
                this.acceleration.Zero();
                this.perturbation.Zero();
                this.geeForce = 0.0;
                this.geeForce_immediate = 0.0;
            }
            this.lastVel = this.obt_velocity;
        }

        private void Update()
        {
            if (this.state == Vessel.State.DEAD)
            {
                return;
            }
            if (this.situation == Vessel.Situations.PRELAUNCH)
            {
                this.launchTime = Planetarium.GetUniversalTime();
            }
            this.missionTime = Planetarium.GetUniversalTime() - this.launchTime;
            //if (this.autoClean && !this.loaded)
            //{
            //    this.Clean(this.autoCleanReason);
            //    return;
            //}
        }

        private void LateUpdate()
        {
            this.CheckControllable();

            if (Time.timeSinceLevelLoad > 0.5f)
            {
                this.updateSituation();
            }
        }

        private void updateSituation()
        {
            if (this.Landed)
            {
                if (this.situation != Vessel.Situations.PRELAUNCH || this.srfSpeed > 2.5)
                {
                    this.situation = Vessel.Situations.LANDED;
                }
                else
                {
                    this.situation = Vessel.Situations.PRELAUNCH;
                }
            }
            else if (this.Splashed)
            {
                this.situation = Vessel.Situations.SPLASHED;
            }
            else if (this.orbit.eccentricity < 1.0 && this.orbit.ApR < this.mainBody.sphereOfInfluence)
            {
                if (this.orbit.PeA < ((!this.mainBody.atmosphere) ? 0.0 : this.mainBody.atmosphereDepth))
                {
                    //if (FlightGlobals.getStaticPressure(FlightGlobals.getAltitudeAtPos(this.vesselTransform.position, this.mainBody), this.mainBody) > 0.0)
                    //{
                    //    this.situation = Vessel.Situations.FLYING;
                    //}
                    //else
                    //{
                    //    this.situation = Vessel.Situations.SUB_ORBITAL;
                    //}
                }
                else
                {
                    this.situation = Vessel.Situations.ORBITING;
                }
            }
            else
            {
                this.situation = Vessel.Situations.ESCAPING;
            }
            if (this.situation != this.lastSituation)
            {
                //GameEvents.onVesselSituationChange.Fire(new GameEvents.HostedFromToAction<Vessel, Vessel.Situations>(this, this.lastSituation, this.situation));
                this.lastSituation = this.situation;
            }
        }

        private void CheckControllable()
        {
            if (!this.loaded)
            {
                this.isControllable = false;
                return;
            }

            //this.isControllable = this.HasControlSources();
            this.isControllable = true;

            /*
            if (this.state == Vessel.State.ACTIVE)
            {
                if (!this.isControllable && !this.noControlLockSet)
                {
                    InputLockManager.SetControlLock(ControlTypes.ALL_SHIP_CONTROLS, "vessel_noControl_" + this.id.ToString());
                    this.noControlLockSet = true;
                }
                if (this.isControllable && this.noControlLockSet)
                {
                    InputLockManager.RemoveControlLock("vessel_noControl_" + this.id.ToString());
                    this.noControlLockSet = false;
                }
            }
            */
        }

        public void ChangeWorldVelocity(Vector3d velOffset)
        {
            if (this.state == Vessel.State.DEAD || this.packed)
            {
                return;
            }

            rb.AddForce(velOffset, ForceMode.VelocityChange);

            /*
            int count = this.parts.get_Count();
            for (int i = 0; i < count; i++)
            {
                Part part = this.parts.get_Item(i);
                if (part.State != PartStates.DEAD && !(part.rb == null))
                {
                    part.rb.AddForce(velOffset, ForceMode.VelocityChange);
                }
            }
            */
        }

        public float GetTotalMass()
        {
            //if (!this.loaded)
            //{
            //    return this.GetUnloadedVesselMass();
            //}

            //float num = 0f;
            float num = rb.mass;

            /*
            int count = this.parts.get_Count();
            for (int i = 0; i < count; i++)
            {
                Part part = this.parts.get_Item(i);
                num += part.mass + part.GetResourceMass();
            }
            */

            return num;
        }

        /*
        private float GetUnloadedVesselMass()
        {
            float num = 0f;
            using (List<ProtoPartSnapshot>.Enumerator enumerator = this.protoVessel.protoPartSnapshots.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    ProtoPartSnapshot current = enumerator.get_Current();
                    num += current.mass + current.resources.Sum((ProtoPartResourceSnapshot x) => float.Parse(x.resourceValues.GetValue("amount")) * PartResourceLibrary.Instance.GetDefinition(x.resourceValues.GetValue("name")).density);
                }
            }
            return num;
        }
        */

        public Vector3 findLocalCenterOfMass()
        {
            //if (!this.loaded)
            //{
                return this.localCoM;
            //}
            //this.centerHelper.Clear();
            //this.recurseCoMs(this.rootPart);
            //return (this.centerHelper.c != 0f) ? this.rootPart.partTransform.InverseTransformPoint(this.centerHelper.center / this.centerHelper.c) : this.rootPart.rb.centerOfMass;
        }

        public Vector3 findWorldCenterOfMass()
        {
            //if (!this.loaded)
            //{
                return this.vesselTransform.TransformPoint(this.localCoM);
            //}
            //this.centerHelper.Clear();
            //this.recurseCoMs(this.rootPart);
            //return (this.centerHelper.c != 0f) ? (this.centerHelper.center / this.centerHelper.c) : this.rootPart.rb.worldCenterOfMass;
        }

        /*
        private void recurseCoMs(Part part)
        {
            if (part.partTransform == null)
            {

            }
            if (part.rb != null)
            {
                this.centerHelper.center += part.rb.worldCenterOfMass * part.rb.mass;
                this.centerHelper.c += part.rb.mass;
            }
            else
            {
                this.centerHelper.center += (part.partTransform.position + part.partTransform.rotation * part.CoMOffset) * part.mass;
                this.centerHelper.c += part.mass;
            }
            int count = part.children.get_Count();
            for (int i = 0; i < count; i++)
            {
                Part part2 = part.children.get_Item(i);
                if (part2.isAttached && part2.State != PartStates.DEAD)
                {
                    this.recurseCoMs(part2);
                }
            }
        }
        */

        public Vector3 findLocalCenterOfPressure()
        {
            if (!this.loaded)
            {
                return Vector3.zero;
            }
            this.centerHelper.Clear();
            this.recurseCoPs();//(this.rootPart);
            //return (this.centerHelper.c != 0f) ? this.rootPart.partTransform.InverseTransformPoint(this.centerHelper.center / this.centerHelper.c) : this.rootPart.rb.centerOfMass;
            return (this.centerHelper.c != 0f) ? this.vesselTransform.InverseTransformPoint(this.centerHelper.center / this.centerHelper.c) : this.rb.centerOfMass;
        }

        private void recurseCoPs()//(Part part)
        {
            //this.centerHelper.center += (part.partTransform.position + part.partTransform.rotation * part.CoMOffset) * part.maximum_drag;
            //this.centerHelper.c += part.maximum_drag;

            this.centerHelper.center += (vesselTransform.position + vesselTransform.rotation.eulerAngles);
            this.centerHelper.c += 1;

            /*
            int count = part.children.get_Count();
            for (int i = 0; i < count; i++)
            {
                Part part2 = part.children.get_Item(i);
                if (part2.isAttached && part2.State != PartStates.DEAD)
                {
                    this.recurseCoMs(part2);
                }
            }
            */
        }

        public Vector3 findLocalMOI()
        {
            return this.findLocalMOI(this.findLocalCenterOfMass());
        }

        public Vector3 findLocalMOI(Vector3 worldCoM)
        {
            //if (!this.loaded)
            //{
            //    return Vector3.zero;
            //}
            this.centerHelper.Clear();
            //this.recurseMOIs(this.rootPart, worldCoM);
            this.MOIs(worldCoM);
            return this.centerHelper.center;
        }

        private void MOIs(Vector3 CoM)
        {
            if (rb != null)
            {
                Vector3 vector = Quaternion.Inverse(this.ReferenceTransform.rotation) * (rb.worldCenterOfMass - CoM);
                this.centerHelper.center += rb.mass * new Vector3(vector.y * vector.y + vector.z * vector.z, vector.x * vector.x + vector.z * vector.z, vector.x * vector.x + vector.y * vector.y);
            }
        }

        private void recurseMOIs()//(Part part, Vector3 CoM)
        {
            /*
            if (part.rb != null)
            {
                Vector3 vector = Quaternion.Inverse(this.ReferenceTransform.rotation) * (part.rb.worldCenterOfMass - CoM);
                this.centerHelper.center += part.mass * new Vector3(vector.y * vector.y + vector.z * vector.z, vector.x * vector.x + vector.z * vector.z, vector.x * vector.x + vector.y * vector.y);
            }
            int count = part.children.get_Count();
            for (int i = 0; i < count; i++)
            {
                Part part2 = part.children.get_Item(i);
                if (part2.isAttached && part2.State != PartStates.DEAD)
                {
                    this.recurseMOIs(part2, CoM);
                }
            }
            */
        }

        private void OnDrawGizmos()
        {
            //Gizmos.color = ((!this.loaded) ? Color.red : Color.black);
            //Gizmos.DrawWireSphere(this.vesselTransform.position, 0.5f);
        }

        public void SetPosition(Vector3 position)
        {
            this.SetPosition(position, this.packed);
        }

        public void SetPosition(Vector3 position, bool usePristineCoords)
        {
            if (usePristineCoords)
            {
                position = position + this.vesselTransform.rotation.eulerAngles;
            }
            else
            {
                this.vesselTransform.position += position - this.vesselTransform.position;
            }

            /*
            if (!this.loaded)
            {
                this.vesselTransform.position = position;
            }
            else if (usePristineCoords)
            {
                int count = this.parts.get_Count();
                for (int i = 0; i < count; i++)
                {
                    Part part = this.parts.get_Item(i);
                    part.partTransform.position = position + this.vesselTransform.rotation * part.orgPos;
                }
            }
            else
            {
                Vector3 b = position - this.vesselTransform.position;
                int count2 = this.parts.get_Count();
                for (int j = 0; j < count2; j++)
                {
                    Part part2 = this.parts.get_Item(j);
                    if (part2.physicalSignificance == Part.PhysicalSignificance.FULL)
                    {
                        part2.partTransform.position += b;
                    }
                }
            }
            */
        }

        public void Translate(Vector3 offset)
        {
            this.SetPosition(offset + this.vesselTransform.position);
        }

        public void SetRotation(Quaternion rotation)
        {
            this.vesselTransform.rotation = rotation;

            /*
            if (!this.loaded)
            {
                this.vesselTransform.rotation = rotation;
            }
            else
            {
                int count = this.parts.get_Count();
                for (int i = 0; i < count; i++)
                {
                    Part part = this.parts.get_Item(i);
                    part.partTransform.rotation = rotation * part.orgRot;
                }
                this.SetPosition(this.vesselTransform.position, true);
            }
            */
        }

        public Transform GetTransform()
        {
            return this.ReferenceTransform;
        }

        public Vector3 GetObtVelocity()
        {
            return this.obt_velocity;
        }

        public Vector3 GetSrfVelocity()
        {
            return this.srf_velocity;
        }

        public Vector3 GetFwdVector()
        {
            return this.ReferenceTransform.forward;
        }

        public Vessel GetVessel()
        {
            return this;
        }

        public string GetName()
        {
            return this.vesselName;
        }

        public Orbit GetOrbit()
        {
            return this.orbit;
        }

        public OrbitDriver GetOrbitDriver()
        {
            return this.orbitDriver;
        }
    }
}