float ThrowSpeedMulitpy	=	(float) 1.45f; 

bool DisableThrownWeaponsCollision = false ; // if true grenades and mines will not impact with any player  unless hit from a shory distance.

bool AccelerateThrownWeapons = 	false; // if false thrown weapons such as grenades / molotovs /  mines will not be accelerated through script.

bool SetMissileOnFire	=	true; 	// set any flammable thrown missile ( chairs, cuesticks,  Suitcase...) to burn in mid air .

bool IncreaseMeleeWeaponMass = false ; // Melee weapons is ten times heavier when thrown.

bool GiveWeaponDefault	 = 	true;		 // gives each player a certain weapons (  cuestick as default).
 
public WeaponItem DefaultWeapon = WeaponItem.KNIFE;// you can change this to any weapon you wish

//  You can find a list of weapons bellow 

bool DisableThrowning 	=	false; 	// missile will not collide do any damage and will be  decelerated (if thrown). might not work in close distances

bool CreateFireCircleOnImpact = false; 	// create a fire circle when the missile is slowed down or no longer  a missle and destroys the missile.

bool CreateExplosionOnImpact = false;//  create a explosion when the missile is slowed down or no longer  a missle and destroys the missile.

bool CreateRandomImpact = false;//  

int RandomProb = 1; // 

string effect = "S_P";

string[] ThrownWeapons = new string[]{ "WpnGrenadesThrown" , "WpnMolotovsThrown",  "WpnMineThrown"};

WeaponItem[] ThrownWeaponsClass = new WeaponItem[]{
	 WeaponItem.GRENADES,
	WeaponItem.MOLOTOVS,	
	WeaponItem.MINES,
	};
WeaponItem[] RifleWeaponsClass = new WeaponItem[]{ 
	WeaponItem.SHOTGUN,
	WeaponItem.TOMMYGUN,		
	WeaponItem.M60,	
	WeaponItem.SNIPER,	
	WeaponItem.SAWED_OFF,	
	WeaponItem.BAZOOKA,	
	WeaponItem.ASSAULT,	
	WeaponItem.FLAMETHROWER,	
	WeaponItem.GRENADE_LAUNCHER,	
	WeaponItem.SMG,	
	WeaponItem.SUB_MACHINEGUN,	
	};
WeaponItem[] HandWeaponsClass = new WeaponItem[]{ 
	WeaponItem.PISTOL,
	WeaponItem.MAGNUM,		
	WeaponItem.UZI,	
	WeaponItem.FLAREGUN,	
	WeaponItem.REVOLVER,	
	WeaponItem.SILENCEDPISTOL,	
	WeaponItem.SILENCEDUZI,	
	};
WeaponItem[] MeleeWeaponsClass = new WeaponItem[]{ 
	WeaponItem.KATANA,
	WeaponItem.PIPE,		
	WeaponItem.MACHETE,	
	WeaponItem.BAT,	
	WeaponItem.AXE,	
	WeaponItem.HAMMER,	
	WeaponItem.BATON,	
	WeaponItem.KNIFE,	
	WeaponItem.CHAIN,
	};
WeaponItem[] MakeShiftWeaponsClass = new WeaponItem[]{ 
	WeaponItem.CHAIR,
	WeaponItem.CHAIR_LEG,		
	WeaponItem.BOTTLE,	
	WeaponItem.BROKEN_BOTTLE,	
	WeaponItem.CUESTICK,	
	WeaponItem.CUESTICK_SHAFT,	
	WeaponItem.SUITCASE,	
	WeaponItem.PILLOW,	
	WeaponItem.FLAGPOLE,
	WeaponItem.TEAPOT,
	};

Random rand = new Random();

public void OnStartup(){
}

if (CreateFireCircleOnImpact || CreateExplosionOnImpact || CreateRandomImpact ){
IObjectTimerTrigger Timer0 = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
Timer0.SetIntervalTime(75);
Timer0.SetRepeatCount(0);
Timer0.SetScriptMethod("veryfast");
Timer0.Trigger();
}
IObjectTimerTrigger Timer = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
Timer.SetIntervalTime(100);
Timer.SetRepeatCount(0);
Timer.SetScriptMethod("fast");
Timer.Trigger();

IObjectTimerTrigger Timer2 = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
Timer2.SetIntervalTime(400);
if (CreateRandomImpact){
Timer2.SetIntervalTime(100);
}
Timer2.SetRepeatCount(0);
Timer2.SetScriptMethod("slow");
Timer2.Trigger();

if (GiveWeaponDefault ){
IObjectTimerTrigger Timer3 = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
Timer3.SetIntervalTime(15000);
Timer3.SetRepeatCount(0);
if (ThrownWeaponsClass.Contains(DefaultWeapon)){
Timer3.SetScriptMethod("mid1");
}
if (RifleWeaponsClass.Contains(DefaultWeapon)){
Timer3.SetScriptMethod("mid2");
}
if (HandWeaponsClass.Contains(DefaultWeapon)){
Timer3.SetScriptMethod("mid3");
}
if (MeleeWeaponsClass.Contains(DefaultWeapon)){
Timer3.SetScriptMethod("mid4");
}
if (MakeShiftWeaponsClass.Contains(DefaultWeapon)){
Timer3.SetScriptMethod("mid5");
}

Timer3.Trigger();
}
}
public void fast(TriggerArgs args){
foreach (IObject missile in Game.GetMissileObjects() ){
if (missile is IObjectWeaponItem){
float speed = 100;
if( (speed > 1|| ((IObjectWeaponItem)missile).WeaponItemType ==  WeaponItemType.Rifle) && ! (missile.CustomId == "ScriptMarkedMissile") &&  (!	(ThrownWeapons.Contains(missile.Name)) ||      AccelerateThrownWeapons ) && !DisableThrowning){
missile.SetLinearVelocity(missile.GetLinearVelocity() * ThrowSpeedMulitpy);
missile.CustomId = "ScriptMarkedMissile";
if ((CreateFireCircleOnImpact || CreateExplosionOnImpact ) ){
IObjectTargetObjectJoint targetjoint = (IObjectTargetObjectJoint)Game.CreateObject ("TargetObjectJoint",missile.GetWorldPosition(),0f,missile.GetLinearVelocity(),missile.GetAngularVelocity ());
targetjoint.SetTargetObject(missile);
targetjoint.CustomId = "destructjoint";
}
if (CreateRandomImpact && rand.Next(0,RandomProb) == 0 ){
int x = rand.Next(0, 25);       //////////////////////////////////
IObjectTargetObjectJoint targetjoint = (IObjectTargetObjectJoint)Game.CreateObject ("TargetObjectJoint",missile.GetWorldPosition(),0f,missile.GetLinearVelocity(),missile.GetAngularVelocity ());
targetjoint.SetTargetObject(missile);

if (x < 25){

targetjoint.CustomId = "Minejoint";
missile.SetLinearVelocity(missile.GetLinearVelocity() * 1.5f);

IObjectTimerTrigger Timer = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
Timer.SetIntervalTime(500);
Timer.SetRepeatCount(1);
Timer.SetScriptMethod("Mine");
Timer.Trigger();


}


if (x >= 9 && x < 15){
targetjoint.CustomId = "Grenadejoint";
missile.SetLinearVelocity(missile.GetLinearVelocity() * 1.5f);

IObjectTimerTrigger Timer = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
Timer.SetIntervalTime(250);
Timer.SetRepeatCount(1);
Timer.SetScriptMethod("grenade");
Timer.Trigger();


}

if (x >= 3 && x < 9){
targetjoint.CustomId = "Bazookajoint";
missile.SetLinearVelocity(missile.GetLinearVelocity() * 1.5f);

IObjectTimerTrigger Timer = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
Timer.SetIntervalTime(250);
Timer.SetRepeatCount(1);
Timer.SetScriptMethod("bazooka");
Timer.Trigger();

}
if (x <= 2){
targetjoint.CustomId = "Electricjoint";
missile.SetLinearVelocity(missile.GetLinearVelocity() * 1.5f);
}
}
if (IncreaseMeleeWeaponMass && ((IObjectWeaponItem)missile).WeaponItemType 	==   WeaponItemType.Melee  ){
missile.SetMass((float)0.8);
}
}
if ((CreateFireCircleOnImpact || CreateExplosionOnImpact ) && speed < 4 && missile.CustomId ==  "ScriptMarkedMissile"){
missile.TrackAsMissile(false);
}

if (DisableThrownWeaponsCollision || DisableThrowning){
if (ThrownWeapons.Contains(missile.Name)){
missile.TrackAsMissile(false);
}
}

if (DisableThrowning){
missile.TrackAsMissile(false);
if (speed > 5){
missile.SetLinearVelocity(missile.GetLinearVelocity() / 3);
}
}


if(SetMissileOnFire && !DisableThrowning && (missile.CustomId == "ScriptMarkedMissile") ){
missile.SetMaxFire();
missile.SetHealth(100);
}
}
}
}
public void slow(TriggerArgs args){
foreach (IObjectTargetObjectJoint joint in Game.GetObjectsByCustomId("Electricjoint") ){
Game.PlayEffect(effect, joint.GetWorldPosition());   
}
foreach (IObject obj in Game.GetObjectsByCustomId("ScriptMarkedMissile") ){
if (!obj.IsMissile && !CreateFireCircleOnImpact ){
obj.CustomId = "";
obj.ClearFire();
if (IncreaseMeleeWeaponMass && ((IObjectWeaponItem)obj).WeaponItemType 	==   WeaponItemType.Melee  ){
obj.SetMass((float)0.08);
}
}
}
}
public void mid1(TriggerArgs args){
foreach (IPlayer ply in Game.GetPlayers()){
if ( ply.CurrentThrownItem.WeaponItem == WeaponItem.NONE && !ply.IsBot && !ply.IsRolling && ! ply.IsDead && !ply.IsStaggering && !ply.IsLayingOnGround){
ply.GiveWeaponItem(DefaultWeapon );
}
}
}
public void mid2(TriggerArgs args){
foreach (IPlayer ply in Game.GetPlayers()){
if ( ply.CurrentPrimaryWeapon.WeaponItem == WeaponItem.NONE && !ply.IsBot && !ply.IsRolling && ! ply.IsDead && !ply.IsStaggering && !ply.IsLayingOnGround){
ply.GiveWeaponItem(DefaultWeapon );
}
}
}
public void mid3(TriggerArgs args){
foreach (IPlayer ply in Game.GetPlayers()){
if ( ply.CurrentSecondaryWeapon.WeaponItem == WeaponItem.NONE && !ply.IsBot && !ply.IsRolling && ! ply.IsDead && !ply.IsStaggering && !ply.IsLayingOnGround){
ply.GiveWeaponItem(DefaultWeapon );
}
}
}
public void mid4(TriggerArgs args){
foreach (IPlayer ply in Game.GetPlayers()){
if ( ply.CurrentMeleeWeapon.WeaponItem == WeaponItem.NONE && !ply.IsBot && !ply.IsRolling && ! ply.IsDead && !ply.IsStaggering && !ply.IsLayingOnGround){
ply.GiveWeaponItem(DefaultWeapon );
}
}
}
public void mid5(TriggerArgs args){
foreach (IPlayer ply in Game.GetPlayers()){
if ( ply.CurrentWeaponDrawn == WeaponItemType.NONE && !ply.IsBot && !ply.IsRolling && ! ply.IsDead && !ply.IsStaggering && !ply.IsLayingOnGround){
ply.GiveWeaponItem(DefaultWeapon );
}
}
}

public void Mine(TriggerArgs args){
foreach (IObjectTargetObjectJoint joint in Game.GetObjectsByCustomId("Minejoint") ){

for (int i =  0; i <= 6 ; i++){
if(joint != null){
Vector2 vec = new Vector2(rand.Next(-3, 3) , rand.Next(-3, 3)) * 4; 
Game.CreateObject("WpnMineThrown" , joint.GetWorldPosition() + vec, 0f , joint.GetLinearVelocity() + vec, 0f  );
}
}
if(joint != null){
joint.GetTargetObject().Destroy();
joint.Destroy();
}
}
}

public void grenade(TriggerArgs args){
foreach (IObjectTargetObjectJoint joint in Game.GetObjectsByCustomId("Grenadejoint") ){
for (int i =  0; i <= 6 ; i++){
if(joint != null){
Vector2 vec = new Vector2(rand.Next(-3, 3) , rand.Next(-3, 3)) * 4; 
Game.CreateObject("WpnGrenadesThrown" , joint.GetWorldPosition() + vec, 0f , joint.GetLinearVelocity() + vec, 0f  );
}
}
if(joint != null){
joint.GetTargetObject().Destroy();
joint.Destroy();
}
}
} 

public void bazooka(TriggerArgs args){
foreach (IObjectTargetObjectJoint joint in Game.GetObjectsByCustomId("Bazookajoint") ){
for (int i =  0; i <= 6 ; i++){
if (joint != null){
Game.SpawnProjectile(ProjectileItem.BAZOOKA, joint.GetWorldPosition(), joint.GetWorldPosition() + joint.GetLinearVelocity())  ;
}
}
if (joint != null){
joint.GetTargetObject().Destroy();
joint.Destroy();
}
}
}

public void veryfast(TriggerArgs args){
if (CreateRandomImpact ){

foreach (IObjectTargetObjectJoint joint in Game.GetObjectsByCustomId("Electricjoint") ){
if (joint.GetTargetObject() != null   ){
if (joint.GetTargetObject().CustomId != "ScriptMarkedMissile"   ){
joint.GetTargetObject().Destroy();
}
}
//////////////////////////////////////////////////////////////////////////
if (joint.GetTargetObject() == null   ){
for (int i =  0; i <= 6 ; i++){
if (joint != null){
Game.PlayEffect("Electric",joint.GetWorldPosition() + new Vector2(rand.Next(-8, 8) , rand.Next(-8, 8)) * 4);
Game.TriggerExplosion(joint.GetWorldPosition() + new Vector2(rand.Next(-8, 8) , rand.Next(-8, 8)) * 4);
}
}
joint.Destroy();
}
}

}

foreach (IObjectTargetObjectJoint joint in Game.GetObjectsByCustomId("destructjoint") ){
if (joint.GetTargetObject() != null   ){
if (joint.GetTargetObject().CustomId != "ScriptMarkedMissile"   ){
 joint.GetTargetObject().Destroy();
Game.TriggerExplosion(joint.GetWorldPosition());
}
}

if (joint.GetTargetObject() == null  ){
if (CreateFireCircleOnImpact ){
Game.SpawnFireNodes(joint.GetWorldPosition(),75, joint.GetLinearVelocity (),2,2,FireNodeType.Flamethrower );
joint.Destroy();
}
if (CreateExplosionOnImpact ){
Game.TriggerExplosion(joint.GetWorldPosition());
joint.Destroy();
}
}
}

foreach (IObjectWeaponItem obj in Game.GetObjectsByCustomId("ScriptMarkedMissile") ){
if (!obj.IsMissile ){
if (CreateFireCircleOnImpact ){
Game.SpawnFireNodes(obj.GetWorldPosition(),75, obj.GetLinearVelocity(),2,2,FireNodeType.Flamethrower  );
if(MakeShiftWeaponsClass.Contains(obj.WeaponItem)){
obj.Destroy();
}
}
if (CreateExplosionOnImpact ){
Game.TriggerExplosion(obj.GetWorldPosition());
if(MakeShiftWeaponsClass.Contains(obj.WeaponItem)){
obj.Destroy();
}
}
obj.CustomId = "";
}
}
}