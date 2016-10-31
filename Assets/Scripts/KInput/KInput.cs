using UnityEngine;

/* Made by @KasperHdL
 * KInput - Cross-platform Coded Action Mapping
 * Version 0.01
 */



namespace KInput{

    public enum Axis{
        StickLeftX   ,
        StickLeftY   ,
        StickRightX  ,
        StickRightY  ,
        DPadX        ,
        DPadY        ,
        TriggerLeft  ,
        TriggerRight ,
        Count
    }

    public enum Button{
        A               ,
        B               ,
        X               ,
        Y               ,
        BumperLeft      ,
        BumperRight     ,
        Start           ,
        Back            ,
        StickLeftClick  ,
        StickRightClick ,
        DPadUp          ,
        DPadDown        ,
        DPadLeft        ,
        DPadRight       ,
        Count
    }


    public class Controller{
        public int controllerIndex = 1;
        protected bool yInverted = false;

        protected int[] axis;
        protected int[] buttons;

        protected Controller(){
            axis = new int[(int)Axis.Count];
            buttons = new int[(int) Button.Count];
        }

        //Getters
        public bool GetButton(Button b){
            int index = buttons[(int)b];
            if(index == -1) return convertAxisToButton(b);
            return Input.GetKey("joystick " + controllerIndex + " button " + index);
        }

        public bool GetButtonDown(Button b){
            int index = buttons[(int)b];
            if(index == -1) return convertAxisToButton(b);
            return Input.GetKeyDown("joystick " + controllerIndex + " button " + index);
        }

        public bool GetButtonUp(Button b){
            int index = buttons[(int)b];
            if(index == -1) return convertAxisToButton(b);
            return Input.GetKeyUp("joystick " + controllerIndex + " button " + index);
        }

        public float GetAxis(Axis a){
            int index = axis[(int) a];
            if(index == -1) return convertButtonToAxis(a);
            float v = Input.GetAxis("joystick " + controllerIndex + " axis " + index);
            if(yInverted && (a == Axis.DPadY || a == Axis.StickLeftY || a == Axis.StickRightY))
                v = -v;
            return v;
        }


        private bool convertAxisToButton(Button b){
            switch(b){
                case Button.DPadUp:
                    return GetAxis(Axis.DPadY) > 0;
                case Button.DPadDown:
                    return GetAxis(Axis.DPadY) < 0;
                case Button.DPadLeft:
                    return GetAxis(Axis.DPadX) < 0;
                case Button.DPadRight:
                    return GetAxis(Axis.DPadX) > 0;
            }
            return false;
        }


        private float convertButtonToAxis(Axis a){
            switch(a){
                case Axis.DPadX:
                    bool l = GetButton(Button.DPadLeft);
                    bool r = GetButton(Button.DPadRight);

                    if(l)
                        return -1f;
                    if(r)
                        return 1f;

                    return 0f;
                    
                case Axis.DPadY:
                    bool d = GetButton(Button.DPadDown);
                    bool u = GetButton(Button.DPadUp);

                    if(d)
                        return -1f;
                    if(u)
                        return 1f;

                    return 0f;
                    
            }
            return 0f;
        }
    }

    public class Xbox360 : Controller{

        public Xbox360(){
            bool isWired = true;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            axis[(int) Axis.StickLeftX]   = 1;
            axis[(int) Axis.StickLeftY]   = 2;
            axis[(int) Axis.StickRightX]  = 4;
            axis[(int) Axis.StickRightY]  = 5;
            axis[(int) Axis.DPadX]        = 6;
            axis[(int) Axis.DPadY]        = 7;
            axis[(int) Axis.TriggerLeft]  = 9;
            axis[(int) Axis.TriggerRight] = 10;
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            yInverted = true;
            axis[(int) Axis.StickLeftX]   = 1;
            axis[(int) Axis.StickLeftY]   = 2;
            axis[(int) Axis.StickRightX]  = 3;
            axis[(int) Axis.StickRightY]  = 4;
            axis[(int) Axis.DPadX]        = -1;//not used
            axis[(int) Axis.DPadY]        = -1;//not used
            axis[(int) Axis.TriggerLeft]  = 5;
            axis[(int) Axis.TriggerRight] = 6;
#elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
            yInverted = true;
            axis[(int) Axis.StickLeftX]   = 1;
            axis[(int) Axis.StickLeftY]   = 2;
            axis[(int) Axis.StickRightX]  = 4;
            axis[(int) Axis.StickRightY]  = 5;
            axis[(int) Axis.DPadX]        = (isWired ? 7 : -1);//only wired
            axis[(int) Axis.DPadY]        = (isWired ? 8 : -1);//only wired
            axis[(int) Axis.TriggerLeft]  = 3;
            axis[(int) Axis.TriggerRight] = 6;
#endif

            //Button

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            buttons[(int) Button.A]               = 0;
            buttons[(int) Button.B]               = 1;
            buttons[(int) Button.X]               = 2;
            buttons[(int) Button.Y]               = 3;
            buttons[(int) Button.BumperLeft]      = 4;
            buttons[(int) Button.BumperRight]     = 5;
            buttons[(int) Button.Start]           = 6;
            buttons[(int) Button.Back]            = 7;
            buttons[(int) Button.StickLeftClick]  = 8;
            buttons[(int) Button.StickRightClick] = 9;
            buttons[(int) Button.DPadUp]          = -1;
            buttons[(int) Button.DPadDown]        = -1;
            buttons[(int) Button.DPadLeft]        = -1;
            buttons[(int) Button.DPadRight]       = -1;



#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            buttons[(int) Button.A]               = 0;
            buttons[(int) Button.B]               = 1;
            buttons[(int) Button.X]               = 2;
            buttons[(int) Button.Y]               = 3;
            buttons[(int) Button.BumperLeft]      = 4;
            buttons[(int) Button.BumperRight]     = 5;
            buttons[(int) Button.Start]           = 6;
            buttons[(int) Button.Back]            = 7;
            buttons[(int) Button.StickLeftClick]  = 8;
            buttons[(int) Button.StickRightClick] = 9;
            buttons[(int) Button.DPadUp]          = 10;
            buttons[(int) Button.DPadDown]        = 11;
            buttons[(int) Button.DPadLeft]        = 12;
            buttons[(int) Button.DPadRight]       = 13;

#elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
            buttons[(int) Button.A]               = 0;
            buttons[(int) Button.B]               = 1;
            buttons[(int) Button.X]               = 2;
            buttons[(int) Button.Y]               = 3;
            buttons[(int) Button.BumperLeft]      = 4;
            buttons[(int) Button.BumperRight]     = 5;
            buttons[(int) Button.Start]           = 6;
            buttons[(int) Button.Back]            = 7;
            buttons[(int) Button.StickLeftClick]  = 8;
            buttons[(int) Button.StickRightClick] = 9;
            buttons[(int) Button.DPadUp]          = (!isWired ? 10 : -1);//only wireless
            buttons[(int) Button.DPadDown]        = (!isWired ? 11 : -1);//only wireless
            buttons[(int) Button.DPadLeft]        = (!isWired ? 12 : -1);//only wireless
            buttons[(int) Button.DPadRight]       = (!isWired ? 13 : -1); //only wireless 
#endif
        }
    }
}
