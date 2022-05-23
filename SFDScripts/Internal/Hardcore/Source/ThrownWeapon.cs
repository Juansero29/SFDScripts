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

        #region Thrown Weapon Class
        public class TThrownWeapon
        {
            public int Id = 0;
            public IObject ThrownWeaponObject = null;
            public WeaponItem Weapon = WeaponItem.NONE;
            public Vector2 Position = new Vector2(0, 0);
            public bool IsDestroyed = false;
            public bool ReadyForRemove = false;
            public int TicksUntilExplosion = 0;
            public int SmokeCount = 0;
            public float RangeForFlashbang = 100;
            public int DurationOfFlashbangStunTime = 200;
            public TThrownWeapon(IObject obj, int id)
            {
                Id = id;
                if (Id == 2 || Id == 3)
                {
                    ThrownWeaponObject = GlobalGame.CreateObject("DrinkingGlass00", obj.GetWorldPosition(), obj.GetAngle(), obj.GetLinearVelocity(), obj.GetAngularVelocity());
                    ObjectsToRemove.Add(ThrownWeaponObject);
                    obj.Remove();
                }
                else
                {
                    ThrownWeaponObject = obj;
                }
                Position = ThrownWeaponObject.GetWorldPosition();
                if (Id == 2 || Id == 3) TicksUntilExplosion = 100;
                if (Id == 2)
                {
                    SmokeCount = 400;
                }
            }
            public void OnDestroyed()
            {
                IsDestroyed = true;
                if (Id == 1)
                {
                    GlobalGame.SpawnFireNodes(Position, 50, new Vector2(0, 0), 3, 10, FireNodeType.Flamethrower);
                }
                if (Id == 0 || Id == 1) ReadyForRemove = true;

                if (Id == 3)
                {
                    FlashbangExplosion();
                }

                if (Id == 2)
                {
                    ThrownWeaponObject = GlobalGame.CreateObject("DrinkingGlass00", Position);
                }
            }
            public void Update()
            {
                if (TicksUntilExplosion > 0) TicksUntilExplosion--;

                if (ThrownWeaponIsNotRemovedOrBeingRemoved()) Position = ThrownWeaponObject.GetWorldPosition();
                else if (!IsDestroyed) OnDestroyed();

                IfDebugShowPlayersHitByThrownWeapon();

                if (TicksUntilExplosion > 0) return;

                if (Id == 2)
                {
                    if (SmokeCount > 0)
                    {
                        SmokeCount--;
                        SpawnSmokeCircle(0);
                        SpawnSmokeCircle(20);
                        TicksUntilExplosion = 2;
                        SmokePlayers(30);
                    }
                    else ReadyForRemove = true;
                }

                if (Id == 3)
                {
                    FlashbangExplosion();
                }
            }

            private bool ThrownWeaponIsNotRemovedOrBeingRemoved()
            {
                return ThrownWeaponObject != null && !ThrownWeaponObject.RemovalInitiated && !ThrownWeaponObject.IsRemoved;
            }

            private void IfDebugShowPlayersHitByThrownWeapon()
            {
                if (!IsDebug) return;
                if (Id == 3)
                {
                    DebugPlayersWhoAreBeingHitByFlash();
                }
            }

            private TPlayer _currentPlayerBeingTested;

            public void FlashbangExplosion()
            {
                PlayExplosionVisualEffects();
                GlobalGame.PlaySound("Explosion", Position, 1);

                StunPlayersInRangeNotCoveredByWall();

                ThrownWeaponObject.Remove();
                ReadyForRemove = true;
            }

            private void PlayExplosionVisualEffects()
            {
                GlobalGame.PlayEffect("EXP", Position);
                GlobalGame.PlayEffect("S_P", Position);
                GlobalGame.PlayEffect("S_P", Position);
            }

            private void StunPlayersInRangeNotCoveredByWall()
            {
                var position = Position + new Vector2(0, 10);
                for (int i = 0; i < PlayerList.Count; i++)
                {
                    _currentPlayerBeingTested = PlayerList[i];
                    if (IsCurrentPlayerBodyDeadOrNull()) continue;

                    var distanceFromPlayerToFlashbang = (_currentPlayerBeingTested.Position - position).Length();
                    if (distanceFromPlayerToFlashbang > RangeForFlashbang) continue;

                    if (!IsPlayerBeingTestedHitByFlashbang()) continue;

                    _currentPlayerBeingTested.StunTime += (int)(DurationOfFlashbangStunTime * (1 - distanceFromPlayerToFlashbang / RangeForFlashbang));
                }
            }

            private bool IsCurrentPlayerBodyDeadOrNull()
            {
                return _currentPlayerBeingTested.User.GetPlayer() == null || _currentPlayerBeingTested.User.GetPlayer().IsDead;
            }

            private void DebugPlayersWhoAreBeingHitByFlash()
            {
                for (int i = 0; i < PlayerList.Count; i++)
                {
                    _currentPlayerBeingTested = PlayerList[i];

                    if (_currentPlayerBeingTested.User.GetPlayer() == null || _currentPlayerBeingTested.User.GetPlayer().IsDead)
                    {
                        continue;
                    }
                    IsPlayerBeingTestedHitByFlashbang();
                }
            }

            private bool IsPlayerBeingTestedHitByFlashbang()
            {
                var rci = CreateFlashbangRayCastInput();
                var distanceBetweenGrenadeAndPlayer = (Position - _currentPlayerBeingTested.Position).Length();
                var raycastResults = Game.RayCast(Position, _currentPlayerBeingTested.Position, rci);
                var isThereStaticGroundBetweenGrenadeAndPlayer = false;

                for (int j = 0; j < raycastResults.Length; j++)
                {
                    var hitResult = raycastResults[j];


                    if (!hitResult.IsPlayer)
                    {
                        isThereStaticGroundBetweenGrenadeAndPlayer = true;
                    }

                    if (hitResult.IsPlayer && !isThereStaticGroundBetweenGrenadeAndPlayer && distanceBetweenGrenadeAndPlayer <= RangeForFlashbang && _currentPlayerBeingTested.Name.Equals(hitResult.HitObject.Name))
                    {
                        if (IsDebug)
                        {
                            Game.DrawCircle(hitResult.Position, 1f, Color.Yellow);
                            Game.DrawLine(hitResult.Position, hitResult.Position + hitResult.Normal * 5f, Color.Yellow);
                            Game.DrawArea(hitResult.HitObject.GetAABB(), Color.Green);
                            Game.DrawText(distanceBetweenGrenadeAndPlayer.ToString(), hitResult.Position, Color.Green);
                        }
                        return true;
                    }
                    else if (!hitResult.IsPlayer)
                    {
                        if (IsDebug)
                        {
                            Game.DrawCircle(hitResult.Position, 1f, Color.Yellow);
                            Game.DrawLine(hitResult.Position, hitResult.Position + hitResult.Normal * 5f, Color.Yellow);
                            Game.DrawArea(hitResult.HitObject.GetAABB(), Color.Red);
                            Game.DrawText(distanceBetweenGrenadeAndPlayer.ToString(), hitResult.Position, Color.Green);
                        }
                    }
                }
                return false;
            }

            private static RayCastInput CreateFlashbangRayCastInput()
            {
                var rci = new RayCastInput()
                {
                    // include the objects that are overlaping the source of the raycast
                    IncludeOverlap = true,
                    // only look at the closest hit
                    ClosestHitOnly = false,
                    // Only hit objects that are hit by projectiles
                    ProjectileHit = RayCastFilterMode.True,
                    // Only hit objects that absorb projectiles
                    AbsorbProjectile = RayCastFilterMode.True,
                    // filter to hit only walls and players
                    MaskBits = CategoryBits.Player + CategoryBits.StaticGround,
                    // activate filter
                    FilterOnMaskBits = true,
                };
                return rci;
            }

            public bool IsRemove()
            {
                return ReadyForRemove;
            }
            public void SpawnSmokeCircle(float radius)
            {
                float angle = 0;
                while (angle < Math.PI * 2)
                {
                    float y = (float)Math.Sin(angle) * radius;
                    float x = (float)Math.Cos(angle) * radius;
                    GlobalGame.PlayEffect("STM", Position + new Vector2(x, y));
                    angle++;
                }
            }
            public void SmokePlayers(float radius)
            {
                for (int i = 0; i < PlayerList.Count; i++)
                {
                    IPlayer pl = PlayerList[i].User.GetPlayer();
                    if (pl == null || pl.IsDead) continue;
                    if ((pl.GetWorldPosition() - Position).Length() <= radius)
                    {
                        PlayerList[i].InSmoke = 3;
                    }
                }
            }
        }


        public static void CreateThrownWeapon(WeaponItem item, int id, Vector2 pos)
        {
            string name = "";
            if (item == WeaponItem.GRENADES) name = "WpnGrenadesThrown";
            else if (item == WeaponItem.MOLOTOVS) name = "WpnMolotovsThrown";
            else if (item == WeaponItem.MINES) name = "WpnMineThrown";
            else if (item == WeaponItem.SHURIKEN) name = "WpnShurikenThrown";
            IObject[] list = GlobalGame.GetObjectsByName(name);
            IObject obj = null;
            float dist = 20;
            for (int i = 0; i < list.Length; i++)
            {
                float currentDist = (list[i].GetWorldPosition() - pos).Length();
                if (currentDist < dist)
                {
                    bool have = false;
                    for (int j = 0; j < ThrownTrackingList.Count; j++)
                    {
                        if (ThrownTrackingList[j].ThrownWeaponObject == list[i])
                        {
                            have = true;
                            break;
                        }
                    }
                    if (have) continue;
                    obj = list[i];
                    dist = currentDist;
                }
            }
            if (obj == null) return;
            ThrownTrackingList.Add(new TThrownWeapon(obj, id));
        }

        public static void ThrownWeaponUpdate()
        {
            for (int i = 0; i < ThrownTrackingList.Count; i++)
            {
                if (ThrownTrackingList[i].IsRemove())
                {
                    ThrownTrackingList.RemoveAt(i);
                    i--;
                }
                else
                {
                    ThrownTrackingList[i].Update();
                }
            }
        }

        #endregion

        /* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

    }
}
