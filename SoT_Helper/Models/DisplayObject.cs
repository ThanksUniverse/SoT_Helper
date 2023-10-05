using SoT_Helper.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Renderer = SoT_Helper.Services.Charm.Renderer;
namespace SoT_Helper.Models
{
    //public struct Vector2
    //{

    //    public Vector2 (float x, float y) 
    //    {
    //        X = x;
    //        Y = y;
    //    }

    //    public float X { get; }
    //    public float Y { get; }
    //}

    public struct DisplayText
    {
        public DisplayText(int size, float offset_X = 0, float offset_Y = 0)
        {
            //TextColor = textColor;
            Size = size;
            Offset_X = offset_X;
            Offset_Y = offset_Y;
        }

        //public Color TextColor { get; set; }
        public int Size { get; set; }
        public float Offset_X { get; }
        public float Offset_Y { get; }
    }

    //public enum Shape
    //{
    //    Circle = 0,
    //    Box = 1,
    //    Line = 2,
    //}

    //public struct Icon
    //{
    //    public Icon(Shape iconShape, float size, Color? iconColor = null, float offset_X = 0, float offset_Y = 0)
    //    {
    //        IconShape = iconShape;
    //        this.size = size;
    //        Offset_X = offset_X;
    //        Offset_Y = offset_Y;
    //        IconColor = iconColor.HasValue ? iconColor.Value : Color.WhiteSmoke;
    //    }

    //    public Shape IconShape { get; set; }
    //    public float size { get; set; }
    //    //public bool Visible { get; set; }
    //    public float Offset_X { get; }
    //    public float Offset_Y { get; }
    //    public Color IconColor { get; set; }

    //}

    public abstract class DisplayObject
    {
        /*
        Parent class to objects like Ship's. Responsible for the base functionality
        of pulling data from our memory objects. These are typically identical
        regardless of the actor type we are looking at; as such would be
        considered "common" and reduces redundant code.
        */

        protected static MemoryReader rm;
        protected int coord_offset;
        protected ulong actor_root_comp_ptr;

        public Vector2? ScreenCoords { get; set; }
        public float Distance { get; set; }

        public int ActorId { get; protected set; }
        public ulong ActorAddress { get; protected set; }
        public string Name { get; set; }
        public string Rawname { get; set; }
        public DisplayText DisplayText { get; set; }
        
        public int Size { get; set; }
        public string Text { get; set; }
        public bool ShowText { get; set; }
        public Color Color { get; set; }
        //public Icon Icon { get; set; }
        public bool ShowIcon { get; set; }
        public bool ToDelete { get; set; }
        public ulong Parent { get; set; }
        public ulong ParentComponent { get; set; }
        private long nextUpdate { get; set; }

        public DisplayObject(MemoryReader memory_reader)
        {
            /*
            Some of our DisplayObject calls need to make memory reads, so we will
            ser out memory reader as a class variable.
            :param memory_reader: The SoT MemoryHelper object we are utilizing to
            read memory data from the game
            */
            rm = memory_reader;
            coord_offset = (int)SDKService.GetOffset("SceneComponent.ActorCoordinates");
        }

        protected int GetActorId(ulong address)
        {
            /*
            Function to get the AActor's ID, used to validate the ID hasn't changed
            while running a "quick" scan
            :param int address: the base address for a given AActor
            :rtype: int
            :return: The AActors ID
            */
            var id = rm.ReadInt((IntPtr)address + (int)SDKService.GetOffset("Actor.actorId"));
            //if(id < 1) return 0;

            return id;
        }

        protected bool CheckRawNameAndActorId(ulong address)
        {
            if(ToDelete)
            {
                return false;
            }

            var id = rm.ReadInt((IntPtr)address + (int)SDKService.GetOffset("Actor.actorId"));
            if (id == 0 || id != ActorId)
            {
                ShowIcon = false;
                ShowText = false;
                ToDelete = true;
                Color = Color.Magenta;
                return false;
            }

  //"SceneComponent.AttachChildren": 200,
  //"SceneComponent.AttachParent": 224,
  //"SceneComponent.MovedActors": 632,
  //"SceneComponent.bHiddenInGame": 240,
  //"SceneComponent.bVisible": 240,
  //"SceneComponent.ComponentVelocity": 572,
  
            if (actor_root_comp_ptr == 0)
            {
                actor_root_comp_ptr = GetRootComponentAddress(address);
            }
            //var children = rm.ReadULong(actor_root_comp_ptr + (ulong)SDKService.GetOffset("SceneComponent.AttachChildren"));
            //int childount = rm.ReadInt(actor_root_comp_ptr + (ulong)SDKService.GetOffset("SceneComponent.AttachChildren"] + 8);
            //var movedActors = rm.ReadULong(actor_root_comp_ptr + (ulong)SDKService.GetOffset("SceneComponent.MovedActors"));
            //var movedActorsCount = rm.ReadInt(actor_root_comp_ptr + (ulong)SDKService.GetOffset("SceneComponent.MovedActors"] + 8);
            //var parent = rm.ReadULong(actor_root_comp_ptr + (ulong)SDKService.GetOffset("SceneComponent.AttachParent"));
            //var bHiddenInGame = rm.ReadBool(actor_root_comp_ptr + (ulong)SDKService.GetOffset("SceneComponent.bHiddenInGame"],5);
            //var bVisible = rm.ReadBool(actor_root_comp_ptr + (ulong)SDKService.GetOffset("SceneComponent.bVisible"],3);
            //var componentVelocity = rm.ReadVector3(actor_root_comp_ptr + (ulong)SDKService.GetOffset("SceneComponent.ComponentVelocity"));
            //if(bHiddenInGame) // 16392 136
            ////if(bVisible != 16392 || bHiddenInGame != 16392) // 16392 136
            //{
            //    var name = Name;
            //    var rawname = Rawname;
            //}
            //if(!bVisible)
            //{
            //    var name = Name;
            //    var rawname = Rawname;
            //}
            //var current_rawname = rm.ReadGname(id);

            //if(current_rawname != Rawname)
            //{
            //    ShowIcon= false;
            //    ShowText= false;
            //    ToDelete= true;
            //    Color= Color.Magenta;
            //    return false;
            //}

            //"Actor.bActorIsBeingDestroyed": 324,
            //"Actor.bHidden": 124,
            bool bIsBeingDestroyed = rm.ReadBool(address + (ulong)SDKService.GetOffset("Actor.bActorIsBeingDestroyed"), 2);
            bool bHidden = rm.ReadBool(address + (ulong)SDKService.GetOffset("Actor.bHidden"),2);
            if (bIsBeingDestroyed)
            {
                ShowIcon = false;
                ShowText = false;
                ToDelete = true;
                Color = Color.Magenta;
                return false;
            }
            if(bHidden)
            {
                ShowIcon = false;
                ShowText = false;
                return false;
            }

            //bool attachReplication = rm.ReadBool(ActorAddress + (ulong)SDKService.GetOffset("Actor.bReplicateAttachment"));

            if (bool.Parse(ConfigurationManager.AppSettings["ShowResourcesOnShips"]) && nextUpdate < DateTime.Now.Ticks)
            {
                nextUpdate = DateTime.Now.AddSeconds(2).Ticks;

                //if (Rawname.Contains("AnyItemCrate"))
                //{

                //}
                //if (this is Actor || (this is StorageContainer && Parent == 0))
                {
                    var actorAddress = ActorAddress;
                    if(Rawname.Contains("Proxy") && !Rawname.Contains("Ship"))
                    {
                        ulong iteminfo = rm.ReadULong(ActorAddress + (ulong)SDKService.GetOffset("ItemProxy.ItemInfo"));
                        if (iteminfo != 0)
                        {
                            var iteminfo_rawname = rm.ReadRawname(iteminfo);
                            if (iteminfo_rawname != "")
                            {
                                actorAddress = iteminfo;
                            }
                        }
                    }

                    ulong attachmentreplication = actorAddress + (ulong)SDKService.GetOffset("Actor.AttachmentReplication");
                    ulong parent = rm.ReadULong(attachmentreplication + (ulong)SDKService.GetOffset("RepAttachment.AttachParent"));
                    Parent = parent;
                    var parentActorId = rm.ReadInt((IntPtr)Parent + (int)SDKService.GetOffset("Actor.actorId"));
                    var parentRawname = rm.ReadGname(parentActorId);
                    ulong parentComponentActor = rm.ReadULong(actorAddress + (ulong)SDKService.GetOffset("Actor.ParentComponentActor"));
                    if (parentComponentActor != 0)
                    {
                        ParentComponent = parentComponentActor;
                        //Parent = parentComponentActor;
                    }
                    if (Parent == 0 || (!SoT_DataManager.Ship_keys.ContainsKey(parentRawname)))
                    {
                        ulong parentComponentActor2 = rm.ReadULong(actorAddress + (ulong)SDKService.GetOffset("Actor.ParentComponentActor"));
                        Parent = parentComponentActor2;
                        parentActorId = rm.ReadInt((IntPtr)Parent + (int)SDKService.GetOffset("Actor.actorId"));
                        parentRawname = rm.ReadGname(parentActorId);
                    }
                }
                //var test = Rawname;
                //    if (Parent != 0 && !bool.Parse(ConfigurationManager.AppSettings["ShowItemsOnShips"]))
                //    {
                //        var parentActorId = rm.ReadInt((IntPtr)Parent + SDKService.GetOffset("Actor.actorId"));
                //        var parentRawname = rm.ReadGname(parentActorId);
                //        if (SoT_DataManager.Ship_keys.ContainsKey(parentRawname))
                //        {
                //            ShowIcon = false;
                //            ShowText = false;
                //            //ToDelete = true;
                //            //Color = Color.Magenta;
                //            return false;
                //        }
                //    }
                //    if (ParentComponent != 0 && !bool.Parse(ConfigurationManager.AppSettings["ShowItemsOnShips"]))
                //    {
                //        var parentActorId = rm.ReadInt((IntPtr)ParentComponent + SDKService.GetOffset("Actor.actorId"));
                //        var parentRawname = rm.ReadGname(parentActorId);
                //        if (SoT_DataManager.Ship_keys.ContainsKey(parentRawname))
                //        {
                //            ShowIcon = false;
                //            ShowText = false;
                //            //ToDelete = true;
                //            //Color = Color.Magenta;
                //            return false;
                //        }
                //    }
            }
            //else if (ShowIcon == false && ShowText == false)
            //    return false;


            return true;
        }

        protected ulong GetRootComponentAddress(ulong address)
        {
            /*
            Function to get an AActor's root component memory address
            :param int address: the base address for a given AActor
            :rtype: int
            :return: the address of an AActors root component
            */
            return rm.ReadULong(address + (ulong)SDKService.GetOffset("Actor.RootComponent"));
        }

        protected Coordinates CoordBuilder(ulong rootCompPtr, int offset)
        {
            /*
            Given an actor, loads the coordinates for that actor
            :param int root_comp_ptr: Actors root component memory address
            :param int offset: Offset from root component to beginning of coords,
            Often determined manually with Cheat Engine
            :rtype: dict
            :return: A dictionary containing the coordinate information
            for a specific actor
            */
            byte[] actorBytes = rm.ReadBytes((UIntPtr)rootCompPtr + (uint)offset, 24);
            float[] unpacked = new float[6];

            for (int i = 0; i < unpacked.Length; i++)
            {
                unpacked[i] = BitConverter.ToSingle(actorBytes, i * 4);
            }

            return new Coordinates()
            {
                x = unpacked[0] / 100,
                y = unpacked[1] / 100,
                z = unpacked[2] / 100,
            }; ;
        }

        protected abstract string BuildTextString();

        public abstract void Update(Coordinates myCoords);

        public abstract void DrawGraphics(Renderer renderer);
        public abstract void DrawGraphics(PaintEventArgs renderer);
    }
}
