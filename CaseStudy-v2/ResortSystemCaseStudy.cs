using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roles {
    // role declaration: a place holder with methods (behavior)
    // declared to populate any object, i.e. any object
    // willing to take this role
    public interface IAdapter { }
    public interface IAdaptee { }
    public interface IContext {
        IState State { get; set; }
    }
    public interface IState { }
}
namespace ResortSystem {
    using Roles;
    public class Services : IAdapter {
        public Services() { }
    }
    public class Admin_Offices { }
    public class Employees { }
    public class Resort {
        // properties
        public Services Services { get; set; }
        public Admin_Offices Admin_offices { get; set; }
        public Employees Employees { get; set; }
    }
    public enum RequestType
    { RoomReservation, RestaurantReservation, ExcursionPkgReservation, 
        EntertainmentPkgReservation }
}
namespace ResortServices {
    using Roles;
    using ResortSystem;
    public class Resort_Services : IAdaptee {
        public Resort_Services() { }
        public virtual bool ReserveResource() { return true; }
    }
    public class Simple_Services : Resort_Services {
        public Simple_Services() { }
    }
    public class RoomAccommodation : Simple_Services, IContext {
        public override bool ReserveResource() {
            Console.WriteLine("room reservation operation");
            return true;
        }
        public IState State { get; set; }
    }
}
namespace Room {
    using Roles;
    public class Room : IState {
        public int roomNumber { get; set; }
        public decimal roomRate { get; set; }
    }
    public class In_Use : Room {
        public In_Use() { Console.WriteLine("In_Use"); }
    }
    public class Available : Room {
        public Available() { Console.WriteLine("Available"); }
    }
    public class Out_of_Service : Room {
        public Out_of_Service() { Console.WriteLine("Out_of_Service"); }
    }
}
namespace CaseStudy {
    using Roles;
    using ResortSystem;
    using ResortServices;
    using Room;
    // Behavior Request() is injected into IAdapter role so
    // now any object who assumes
    // this role, will have this method. Using
    // extension method to add "Request()"
    // method to objects involved in the request service
    public static class RequestTrait {
        public static bool Request(this IAdapter adapter, IAdaptee adaptee, RequestType request) {
            bool rc = false;
            switch (request) {
                case RequestType.RoomReservation:
                    Resort_Services ra = adaptee as RoomAccommodation;
                    rc = ra.ReserveResource();
                    break;
                default:
                    Console.WriteLine("{0}: unrecognized request", request);
                    rc = false;
                    break;
            }
            return (rc);
        }
    }
    // Behavior Handle() is injected into State objects
    public static class HandleTrait {
        public static bool Handle(this IState state, IContext ctxt) {
            bool rc = false;
            Type tt = ctxt.State.GetType();
            string typeName = tt.ToString();
            switch (typeName) {
                case "Room.Available":
                    ctxt.State = new In_Use();
                    break;
                case "Room.In_Use":
                    ctxt.State = new Out_of_Service();
                    break;
                case "Room.Out_of_Service":
                    ctxt.State = new Available();
                    rc = true;
                    break;
                default: break;
            }
            return (rc);
        }
    }
    // Our collaboration model mimics the use case in DCI.
    // The context in which the RequestResource
    // (room reservation) use case is executed is this:
    public class RequestResourceContext {
        // properties for accessing the concrete objects
        // relevant in this context through their
        // methodless roles
        public IAdaptee Adaptee { get; private set; }
        public IAdapter Adapter { get; private set; }
        public RequestType ReqType { get; private set; }

        public RequestResourceContext(IAdapter adapter, IAdaptee adaptee,
            RequestType resource) {
            Adaptee = adaptee;
            Adapter = adapter;
            ReqType = resource;
        }
        public bool Doit() {
            bool rc = Adapter.Request(Adaptee, ReqType);
            return (rc);
        }
    }
    public class HandleRoomAccomationContext
    {
        public IState State { get; private set; }
        public IContext Context { get; private set; }
        public HandleRoomAccomationContext(IContext ctxt, IState state) {
            State = state;
            Context = ctxt;
        }
        public bool Doit() {
            bool rc = State.Handle(Context);
            return (rc);
        }
    }
    class ResortSystemCaseStudy {
        static void Main(string[] args) {
            // demonstrate Adapter pattern integration
            Services services = new Services();
            Simple_Services ra = new RoomAccommodation() { State = new Available() };
            RequestResourceContext integration = new RequestResourceContext(services, ra, RequestType.RoomReservation);
            bool rc = integration.Doit();
            // demonstrate State pattern integration
            RoomAccommodation ra2 = new RoomAccommodation() { State = new Available() };
            HandleRoomAccomationContext integration_2 = new HandleRoomAccomationContext(ra2, ra2.State);
            bool rc2 = integration_2.Doit();
            rc2 = integration_2.Doit();
            rc2 = integration_2.Doit();
            Console.WriteLine("press any key to exit...");
            Console.ReadKey();
        }
    }
}
