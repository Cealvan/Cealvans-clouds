
using Celeste64;
using Foster.Framework;

using Vec3 = System.Numerics.Vector3;
using Matrix = System.Numerics.Matrix4x4;

namespace Cealvan.Cloud
{
    public class CealvansClouds : GameMod
    {

        public override void OnModLoaded()
        {
            base.OnModLoaded();
            Map.ActorFactory cloudFactory = new((map, entity) => new Cloud()) { IsSolidGeometry = true};
            AddActorFactory("cloud", cloudFactory);
        }
    }






    public class Cloud: Solid, IHaveModels
    {
        public SkinnedModel? Model;

        private Player? rider = null;
        private float ogJumpSpeed = 0.0f;
        private bool isAnimating = false;
        private float animationTime = 0.0f;
        private int currentFrame = 0;
        public Vec3 Start = Vec3.Zero;


        public override void Added()
        {
            Start = Position;
            Climbable = false;
            Model = new SkinnedModel(Assets.Models["cloud"]);
            Model.Transform = Matrix.CreateScale(8.0f);
            base.Added();
        }


        private class KeyFrame
        {
            public float time, pos, jumpMult;
            public string action;
            public KeyFrame(float time, float pos, float jumpMult, string action = "")
            {
                this.time = time;
                this.pos = pos;
                this.jumpMult = jumpMult;
                this.action = action;
            }
        }

        private List<KeyFrame> keyFrames = [
        //              time   pos   Jump   action
            new KeyFrame(0.0f, 0.0f, 1.0f),
            new KeyFrame(0.3f, -0.7f, 0.2f),
            new KeyFrame(0.6f, 0.2f, 1.5f),
            new KeyFrame(0.6f, 0.3f, 1.5f),
            new KeyFrame(0.7f, 0.5f, 1.5f, "jump"),
            new KeyFrame(1.0f, 0.0f, 1.0f)
        ];


        public override void Update()
        {
            base.Update();

            if (isAnimating)
            {
                animationTime += Time.Delta;
                if (animationTime > keyFrames[currentFrame + 1].time)
                {
                    currentFrame++;
                    if (keyFrames[currentFrame].action == "jump" && rider != null)
                    {
                        rider.Jump();
                    }
                }
                if (currentFrame == keyFrames.Count - 1)
                {
                    animationTime = 0.0f;
                    currentFrame = 0;
                    isAnimating = false;
                }
                else
                {

                    float timeSinceFrameStart = animationTime - keyFrames[currentFrame].time;
                    float frametime = keyFrames[currentFrame + 1].time - keyFrames[currentFrame].time;
                    float framePercent = timeSinceFrameStart / frametime;
                    float positionTarget = getTarget(keyFrames[currentFrame].pos, keyFrames[currentFrame + 1].pos, framePercent);
                    float JumpMultTarget = getTarget(keyFrames[currentFrame].jumpMult, keyFrames[currentFrame + 1].jumpMult, framePercent);

                    Model.Transform = Matrix.CreateTranslation(0, 0, positionTarget);
                    Model.Transform *= Matrix.CreateScale(8.0f);

                    if (rider != null)
                    {
                        rider.JumpSpeed = ogJumpSpeed * JumpMultTarget;
                    }
                }

            }

            if (HasPlayerRider())
            {
                isAnimating = true;
                if (rider == null)
                {
                    rider = world.Get<Player>();
                    ogJumpSpeed = rider.JumpSpeed;
                }
            }
            else
            {
                if (rider != null)
                {
                    rider.JumpSpeed = ogJumpSpeed;
                    rider = null;
                }
            }
        }

        private float getTarget(float start, float end, float framePercent)
        {

            float difference = end - start;
            float percent = difference * framePercent;
            float target = start + percent;

            return target;
        }

        public virtual void CollectModels(List<(Actor Actor, Model Model)> populate)
        {
            if (Model != null)
                populate.Add((this, Model));
        }

    }
}
