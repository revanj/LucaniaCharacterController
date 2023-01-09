using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Player
{
    // struct that wraps all inputs
    private class InputState
    {
        public Vector2 WalkDirection;
        public bool Jump;
        public bool JumpHeld;
    }

    // struct that wraps all configs, e.g. walk speed
    private class ConfigState
    {
        public const float WalkSpeed = 4f;
        public const float Gravity = 8f;
        public const float JumpInc = 8f;
        public const float JumpHeight = 2.5f;
        public const float SlideSpeed = 2f;
        public const float WallJumpLimitH = 2f;
        public const float WallJumpForceH = 2f;
        public const float WallJumpForceV = 2f;
    }

    private class CheckState
    {
        public bool IsOnFloor;
        public int IsOnWall;
    }
}
