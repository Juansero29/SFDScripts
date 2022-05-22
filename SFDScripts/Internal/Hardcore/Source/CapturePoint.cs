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

        #region Capture Point Class
        public class TCapturePoint
        {
            public IObjectText Object;
            public int DefaultCaptureProgress = 0;
            public int CaptureProgress = 0;

            /// <summary>
            /// Builds a capture point
            /// </summary>
            /// <param name="obj">The text that shows the capture point</param>
            /// <param name="captureProgress">The capture progress. -100 for red team, 100 for blue team, 0 for neutral</param>
            public TCapturePoint(IObjectText obj, int captureProgress)
            {
                Object = obj;
                DefaultCaptureProgress = captureProgress;
            }
            public void Start()
            {
                CaptureProgress = DefaultCaptureProgress;
            }
            public void Update()
            {
                for (int i = 0; i < PlayerList.Count; i++)
                {
                    IPlayer pl = PlayerList[i].User.GetPlayer();
                    if (pl != null && !pl.IsDead)
                    {
                        if (TestDistance(pl.GetWorldPosition(), Object.GetWorldPosition(), CapturePointRadius))
                        {
                            if (pl.GetTeam() == PlayerTeam.Team1)
                            {
                                if (CaptureProgress < MaxCapturePointProgress)
                                {
                                    CaptureProgress++;
                                    PlayerList[i].AddExp(0.05f, 2);
                                    if (CaptureProgress == MaxCapturePointProgress - 5)
                                    {
                                        GlobalGame.PlaySound("MenuOK", pl.GetWorldPosition(), 1f);
                                    }
                                }
                            }
                            else
                            {
                                if (CaptureProgress > -MaxCapturePointProgress)
                                {
                                    CaptureProgress--;
                                    PlayerList[i].AddExp(0.05f, 2);
                                    if (CaptureProgress == -MaxCapturePointProgress + 5)
                                    {
                                        GlobalGame.PlaySound("MenuOK", pl.GetWorldPosition(), 1f);
                                    }
                                }
                            }


                        }
                    }
                }
                float percent = ((float)Math.Abs(CaptureProgress)) / ((float)MaxCapturePointProgress);
                byte color = (byte)(255f * percent);
                if (CaptureProgress >= 0)
                {
                    Object.SetTextColor(new Color((byte)(255 - color), (byte)(255 - color), 255));
                }
                else
                {
                    Object.SetTextColor(new Color(255, (byte)(255 - color), (byte)(255 - color)));
                }

            }
        }

        #endregion

        /* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

    }
}
