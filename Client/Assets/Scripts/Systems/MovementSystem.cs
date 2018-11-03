using Components;
using Unity.Entities;
using UnityEngine;

namespace Systems
{
  public class MovementSystem : ComponentSystem
  {
    private struct Group
    {
      public Rigidbody Rigidbody;
      public PlayerInput Input;
      public Speed Speed;
    }
    
    protected override void OnUpdate()
    {
      foreach (var entity in GetEntities<Group>())
      {
        entity.Rigidbody.AddForce(0f, 0f, entity.Input.Vertical * entity.Speed.Value);
        entity.Rigidbody.rotation.SetFromToRotation(Vector3.zero, new Vector3(0f, 0f, entity.Input.Horizontal * 100));
      }
    }
  }
}