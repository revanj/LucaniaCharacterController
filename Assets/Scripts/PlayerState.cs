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
      public override void OnEnter(Player player)
      {
         player._doubleJumpCount = ConfigState.DoubleJumpCap;
      }
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
         var jumpUp = Vector2.up * (ConfigState.JumpInc * Time.deltaTime);
         player._body.MovePosition(player._body.position + walkVec + jumpUp);

         if (input.Jump && player._doubleJumpCount > 0)
         {
            return new DoubleJumpState();
         }
         if (input.JumpHeld && player.transform.position.y - _jumpStartHeight >= ConfigState.JumpHeight)
         {
            return new InAirDownState();
         }

         if (!input.JumpHeld)
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
         if (check.IsOnWall != 0) { return new WallSlideState(); }

         if (input.Jump && player._doubleJumpCount > 0)
         {
            return new DoubleJumpState();
         }

         return null;
      }
   }
   
   private class WallSlideState : PlayerState
   {
      public override void OnEnter(Player player)
      {
         base.OnEnter(player);
         player._doubleJumpCount = ConfigState.DoubleJumpCap;
      }

      public override PlayerState Execute(Player player, InputState input, ConfigState config, CheckState check)
      {
         if (Math.Sign(input.WalkDirection.x) != 0 && Math.Sign(input.WalkDirection.x) != check.IsOnWall)
         {
            return new InAirDownState();
         }
         
         var slideVec = new Vector2(0, -ConfigState.SlideSpeed * Time.deltaTime);
         player._body.MovePosition(player._body.position + slideVec);
         
         // state transitions
         if (input.Jump) { return new WallJumpState(check.IsOnWall); }
         if (check.IsOnFloor) { return new OnFloorState(); }
         
         return null;
      }
   }
   
   
   private class WallJumpState : PlayerState
   {
      private int _wallDir;
      private float _startingX;
      private bool _folded = false;
      public WallJumpState(int wallDir)
      {
         _wallDir = -wallDir;
      }

      public override void OnEnter(Player player)
      {
         _startingX = player.transform.position.x;
      }
      

      public override PlayerState Execute(Player player, InputState input, ConfigState config, CheckState check)
      {
         
         // -1 means wall on the left
         if (!_folded)
         {
            player._body.MovePosition(player._body.position + 
                                      new Vector2(ConfigState.WallJumpForceH * _wallDir, 
                                         ConfigState.WallJumpForceV) * Time.deltaTime);
            if (Mathf.Abs(player.transform.position.x - _startingX) >= ConfigState.WallJumpLimitH)
            {
               _folded = true;
            }
         }
         else
         {
            player._body.MovePosition(player._body.position + 
                                      new Vector2(-ConfigState.WallJumpForceH * _wallDir, 
                                         ConfigState.WallJumpForceV) * Time.deltaTime);
            
            if (check.IsOnWall == _wallDir)
            {
               return new WallSlideState();
            }
            if ((player.transform.position.x - _startingX) * _wallDir < 0)
            {
               return new InAirDownState();
            }
         }
         
         // state transitions
         
         if (check.IsOnFloor) { return new OnFloorState(); }
         if (input.JumpHeld) { return null; }

         return new InAirDownState();
      }
   }
   
   private class DoubleJumpState : PlayerState
   {
      private float _enterHeight;
      private bool _dipCompleted;
      
      public override void OnEnter(Player player)
      {
         base.OnEnter(player);
         _enterHeight = player.transform.position.y;
         player._doubleJumpCount -= 1;
         _dipCompleted = false;
      }

      public override PlayerState Execute(Player player, InputState input, ConfigState config, CheckState check)
      {
         var walkVec = new Vector2(Math.Sign(input.WalkDirection.x) * ConfigState.WalkSpeed * Time.deltaTime, 0);
         if (!_dipCompleted)
         {
            var gravity = Vector2.down * (ConfigState.Gravity * Time.deltaTime);
            player._body.MovePosition(player._body.position + walkVec + gravity);
            if (_enterHeight - player.transform.position.y > ConfigState.DoubleJumpDip)
            {
               _dipCompleted = true;
            }
         }
         else
         {
            var lift = Vector2.up * (ConfigState.DoubleJumpInc * Time.deltaTime);
            player._body.MovePosition(player._body.position + lift + walkVec);
            if (player.transform.position.y - _enterHeight > ConfigState.DoubleJumpHeight)
            {
               return new InAirDownState();
            }
         }
         
         
         // state transitions
         if (check.IsOnFloor) { return new OnFloorState(); }
         if (input.JumpHeld) { return null; }

         return new InAirDownState();
      }
   }

}
