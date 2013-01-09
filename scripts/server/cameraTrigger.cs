function CameraRoomTrigger::onEnterTrigger(%this,%trigger,%obj)
{
   // This method is called whenever an object enters the %trigger
   // area, the object is passed as %obj.
   // echo(" -- Trigger entered");
   
   %client = %obj.client;
   
   switch$(%trigger.name)
   {
      case "Room1":
         //echo(" - entered room 1");
         %client.camera.position = Cam1.position;
         %client.camera.rotation = Cam1.rotation;
      
      case "Room2":
         //echo(" - entered room 2");
         %client.camera.position = Cam2.position;
         %client.camera.rotation = Cam2.rotation;

      case "Room3":
         //echo(" - entered room 3");
         %client.camera.position = Cam3.position;
         %client.camera.rotation = Cam3.rotation;

      case "Room4":
         //echo(" - entered room 4");
         %client.camera.position = Cam4.position;
         %client.camera.rotation = Cam4.rotation;

      case "Room5":
         //echo(" - entered room 5");
         %client.camera.position = Cam5.position;
         %client.camera.rotation = Cam5.rotation;

      case "Room6":
         //echo(" - entered room 6");
         %client.camera.position = Cam6.position;
         %client.camera.rotation = Cam6.rotation;

      case "Room7":
         //echo(" - entered room 7");
         %client.camera.position = Cam7.position;
         %client.camera.rotation = Cam7.rotation;
         
      default:
         echo(" -- Something is wrong with your room triggers");
   }
}
