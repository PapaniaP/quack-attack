public class OneMoreQuackEffect : IAcquisitionEffect
{
  public void ApplyEffect()
  {
    GameManager.Instance.AddLife(1);
    UnityEngine.Debug.Log("💚 One More Quack granted +1 life!");
  }
}