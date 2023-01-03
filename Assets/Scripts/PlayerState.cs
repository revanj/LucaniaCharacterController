using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Player
{
   private abstract class PlayerState
   {
      // those have default "do nothing"
      public virtual void OnEnter(Player player) { }
      public virtual void OnExit() { }

      // Execute function must be overridden
      public abstract PlayerState Execute(Player player, InputState input, ConfigState config, CheckState check);
   }

   private class OnFloorState : PlayerState
   {
      public override void OnEnter(Player player) { }
      public override PlayerState Execute(Player player, InputState input, ConfigState config, CheckState check)
      {
         var walkVec = new Vector2(Math.Sign(input.WalkDirection.x) * ConfigState.WalkSpeed * Time.deltaTime, 0);
         player._body.MovePosition(player._body.position + walkVec);
         
         // state transitions
         if (!check.IsOnFloor) { return new InAirDownState(); }
         if (input.Jump) { return new InAirUpState(); }

         return null;
      }
      
   }
   
   private class InAirUpState : PlayerState
   {
      private float _jumpStartHeight;

      public override void OnEnter(Player player)
      {
         _jumpStartHeight = player.transform.position.y;
      }
      public override PlayerState Execute(Player player, InputState input, ConfigState config, CheckState check)
      {
         var walkVec = new Vector2(Math.Sign(input.WalkDirection.x) * ConfigState.WalkSpeed * Time.deltaTime, 0);
         var jumpUp = Vector2.up * (config.JumpInc * Time.deltaTime);
         if (input.JumpHeld && player.transform.position.y - _jumpStartHeight < config.JumpHeight)
         {
            player._body.MovePosition(player._body.position + walkVec + jumpUp);
         }
         else
         {
            return new InAirDownState();
         }
         return null;
      }
   }

   private class InAirDownState : PlayerState
   {
      public override PlayerState Execute(Player player, InputState input, ConfigState config, CheckState check)
      {
         var walkVec = new Vector2(Math.Sign(input.WalkDirection.x) * ConfigState.WalkSpeed * Time.deltaTime, 0);
         var gravity = Vector2.down * (ConfigState.Gravity * Time.deltaTime);
         player._body.MovePosition(player._body.position + walkVec + gravity);
         
         // state transitions
         if (check.IsOnFloor) { return new OnFloorState(); }
         
         return null;
      }
   }

}
