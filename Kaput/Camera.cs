using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Kaput
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Camera : GameComponent
    {
        protected Vector3 m_position = new Vector3(0, 0, 10);
        protected Vector3 m_up = Vector3.Up;
        protected Vector3 m_direction;

        protected Quaternion m_rotation = Quaternion.Identity;

        protected const float m_pitchLimit = 1.4f;

        protected const float m_nearPlaneDistance = 0.01f;
        protected const float m_farPlaneDistance = 100;

        protected const float m_speed = 0.25f;
        protected const float m_rotationSpeed = m_speed / 10;

        protected int m_windowWidth;
        protected int m_windowHeight;
        protected float m_aspectRatio;
        protected MouseState m_prevMouse;


        /// <summary>
        /// Creates the instance of the camera.
        /// </summary>
        /// <param name="game">Provides graphics device initialization, game logic, 
        /// rendering code, and a game loop.</param>
        public Camera(Game game)
            : base(game)
        {
            m_windowWidth = Game.Window.ClientBounds.Width;
            m_windowHeight = Game.Window.ClientBounds.Height;
            m_aspectRatio = (float)m_windowWidth / (float)m_windowHeight;

            // Create the direction vector and normalize it since it will be used for movement
            m_direction = Vector3.Zero - m_position;
            m_direction.Normalize();

            // Create default camera matrices
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, m_aspectRatio, m_nearPlaneDistance, m_farPlaneDistance);
            View = CreateLookAt();

            // Use isometric view by default
            MoveTo(new Vector3(0, 10, 0), MathHelper.ToRadians(45), MathHelper.ToRadians(30));
        }


        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            m_prevMouse = Mouse.GetState();

            base.Initialize();
        }


        /// <summary>
        /// Handle the camera movement using user input.
        /// </summary>
        protected virtual void ProcessInput()
        {
            var mouse = Mouse.GetState();
            var keyboard = Keyboard.GetState();

            // Disregard Y coordinate, since we just want to move parallel to the ground
            Vector3 rotation = new Vector3(m_direction.X, 0, m_direction.Z);

            // Move camera with WASD keys
            if (keyboard.IsKeyDown(Keys.W))
                m_position += rotation * m_speed;

            if (keyboard.IsKeyDown(Keys.S))
                m_position -= rotation * m_speed;

            if (keyboard.IsKeyDown(Keys.A))
                // Rotate the camera when holding a Shift
                if (keyboard.IsKeyDown(Keys.LeftShift))
                    Yaw = -m_rotationSpeed;
                // Strafe by adding a cross product of m_up and m_direction vectors
                else m_position += Vector3.Cross(m_up, m_direction) * m_speed;

            if (keyboard.IsKeyDown(Keys.D))
                if (keyboard.IsKeyDown(Keys.LeftShift))
                    Yaw = m_rotationSpeed;
                else m_position -= Vector3.Cross(m_up, m_direction) * m_speed;

            if (mouse != m_prevMouse)
            {
                // Zoom with a mouse wheel
                if (mouse.ScrollWheelValue > m_prevMouse.ScrollWheelValue)
                    if (keyboard.IsKeyDown(Keys.LeftShift))
                        Pitch = m_rotationSpeed;
                    else m_position -= m_up * m_speed;
                else if (mouse.ScrollWheelValue < m_prevMouse.ScrollWheelValue)
                    if (keyboard.IsKeyDown(Keys.LeftShift))
                        Pitch = -m_rotationSpeed;
                    else m_position += m_up * m_speed;

                m_prevMouse = Mouse.GetState();
            }
        }


        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // Handle camera movement
            ProcessInput();

            View = CreateLookAt();

            base.Update(gameTime);
        }


        /// <summary>
        /// Creates a rotation Quaternion.
        /// </summary>
        /// <param name="yaw">Change to the yaw, side to side movement of the camera.</param>
        /// <param name="pitch">Change to the pitch, up and down movement of the camera.</param>
        /// <param name="roll">Change to the roll, barrel rotation of the camera.</param>
        /// <param name="direction">Direction vector.</param>
        /// <param name="up">Up vector.</param>
        /// <returns></returns>
        protected Quaternion CreateRotation(float yaw, float pitch, float roll, Vector3 direction, Vector3 up)
        {
            var output = Quaternion.CreateFromAxisAngle(up, yaw) *
                Quaternion.CreateFromAxisAngle(Vector3.Cross(up, direction), pitch) *
                Quaternion.CreateFromAxisAngle(direction, roll);
            output.Normalize();
            return output;
        }


        /// <summary>
        /// Create a view matrix using camera vectors.
        /// </summary>
        protected Matrix CreateLookAt()
        {
            return Matrix.CreateLookAt(m_position, m_position + m_direction, m_up);
        }


        /// <summary>
        /// Move the camera to the given position, yaw and pitch.
        /// </summary>
        /// <param name="position">A destination position of the camera.</param>
        /// <param name="yaw">Yaw of the camera in radians.</param>
        /// <param name="pitch">Pitch of the camera in radians.</param>
        public void MoveTo(Vector3 position, float yaw, float pitch)
        {
            Position = position;
            Yaw = yaw;
            Pitch = pitch;
        }


        /// <summary>
        /// Position vector.
        /// </summary>
        public Vector3 Position
        {
            get { return m_position; }
            protected set { m_position = value; }
        }


        /// <summary>
        /// Yaw of the camera in radians.
        /// </summary>
        public float Yaw
        {
            get { return (float)(Math.PI - Math.Atan2(m_direction.X, m_direction.Z)); }
            protected set
            {
                m_rotation = Quaternion.CreateFromAxisAngle(m_up, -value);
                m_direction = Vector3.Transform(m_direction, m_rotation);
            }
        }


        /// <summary>
        /// Pitch of the camera in radians.
        /// </summary>
        public float Pitch
        {
            get { return (float)Math.Asin(m_direction.Y); }
            protected set
            {
                m_rotation = Quaternion.CreateFromAxisAngle(Vector3.Cross(m_up, m_direction), value);
                m_direction = Vector3.Transform(m_direction, m_rotation);
            }
        }


        /// <summary>
        /// View matrix accessor.
        /// </summary>
        public Matrix View { get; set; }


        /// <summary>
        /// Projection matrix accessor.
        /// </summary>
        public Matrix Projection { get; set; }
    }
}
