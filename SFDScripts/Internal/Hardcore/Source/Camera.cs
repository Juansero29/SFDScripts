using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SFDGameScriptInterface;


namespace SFDScript
{

    public partial class GameScript : GameScriptInterface
    {

        /* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */

        #region Camera Settings


        /// <summary>
        /// The camera position. When changed, the camera will go to this position at the <see cref="CameraSpeed"/> set. 
        /// It will be updated on the <see cref="UpdateCamera"/> method.
        /// </summary>
        public static Vector2 CameraPosition;

        /// <summary>
        /// The speed used to move the camera each time it is updated in the <see cref="UpdateCamera"/> method
        /// </summary>
        public static float CameraSpeed = 2.0f;
        #endregion

        #region Camera Methods
        public void UpdateCamera()
        {
            Area cameraArea = Game.GetCameraArea();
            if (cameraArea.Left != CameraPosition.X)
            {
                if (Math.Abs(cameraArea.Left - CameraPosition.X) <= CameraSpeed)
                {
                    cameraArea.Left = CameraPosition.X;
                }
                else if (cameraArea.Left < CameraPosition.X)
                {
                    cameraArea.Left += CameraSpeed;
                }
                else
                {
                    cameraArea.Left -= CameraSpeed;
                }
                cameraArea.Right = cameraArea.Left + CameraWidth;
            }
            if (cameraArea.Top != CameraPosition.Y)
            {
                if (Math.Abs(cameraArea.Top - CameraPosition.Y) <= CameraSpeed)
                {
                    cameraArea.Top = CameraPosition.Y;
                }
                else if (cameraArea.Top < CameraPosition.Y)
                {
                    cameraArea.Top += CameraSpeed;
                }
                else
                {
                    cameraArea.Top -= CameraSpeed;
                }
                cameraArea.Bottom = cameraArea.Top - CameraHeight;
            }
            Game.SetCameraArea(cameraArea);
            //Game.SetBorderArea(cameraArea);
        }

        #endregion

        /* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

    }
}
