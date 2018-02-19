// -----------------------------------------------------------------------
// <file>World.cs</file>
// <copyright>Grupa za Grafiku, Interakciju i Multimediju 2013.</copyright>
// <author>Srđan Mihić</author>
// <author>Aleksandar Josić</author>
// <summary>Klasa koja enkapsulira OpenGL programski kod.</summary>
// -----------------------------------------------------------------------
using System;
using System.Drawing;
using System.Drawing.Imaging;
using SharpGL.SceneGraph.Quadrics;
using SharpGL.SceneGraph.Core;
using SharpGL;

namespace AssimpSample
{


    /// <summary>
    ///  Klasa enkapsulira OpenGL kod i omogucava njegovo iscrtavanje i azuriranje.
    /// </summary>
    public class World : IDisposable
    {
        #region Atributi

        /// <summary>
        ///	 Ugao rotacije Meseca
        /// </summary>
        private float m_moonRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije Zemlje
        /// </summary>
        private float m_earthRotation = 0.0f;

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        private AssimpScene m_scene;

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        private float m_xRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        private float m_yRotation = 0.0f;

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        private float m_sceneDistance = 500.0f;

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height;

        private uint[] m_textures = null;
        private enum TextureObjects { Plastic = 0, Ball, Grass};

        private readonly int m_textureCount = Enum.GetNames(typeof(TextureObjects)).Length;

        private string[] m_textureFiles = {
            "..//..//images//plastic.jpg",
            "..//..//images//ball.jpg",
            "..//..//images//grass.jpg"
        };
        private double scaleBall;
        private double scaleBall0 = 20;
        private double rotateBall0 = 5;
        private double rotateBall;
        private float[] ambColor = { 0.5f, 0.5f, 0.5f , 0.3f};
        private double pomeraj = 2;
        private Boolean gore;
        private Boolean dole;
        private int count;
        private double zoom;
        private bool shooting;
        private float step;
        private MainWindow main;


        #endregion Atributi

        #region Properties

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        public AssimpScene Scene
        {
            get { return m_scene; }
            set { m_scene = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        public float RotationX
        {
            get { return m_xRotation; }
            set { m_xRotation = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        public float RotationY
        {
            get { return m_yRotation; }
            set { m_yRotation = value; }
        }

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        public float SceneDistance
        {
            get { return m_sceneDistance; }
            set { m_sceneDistance = value; }
        }

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }
        public double Zoom
        {
            get { return zoom; }
            set { zoom = value; }
        }
        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        public int Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        #endregion Properties

        #region Konstruktori

        /// <summary>
        ///  Konstruktor klase World.
        /// </summary>
        public World(String scenePath, String sceneFileName, int width, int height, OpenGL gl)
        {
            this.m_scene = new AssimpScene(scenePath, sceneFileName, gl);
            this.m_width = width;
            this.m_height = height;
            m_textures = new uint[m_textureCount];
        }

        /// <summary>
        ///  Destruktor klase World.
        /// </summary>
        ~World()
        {
            this.Dispose(false);
        }

        internal void startShooting(MainWindow mw)
        {
            shooting = true;
            main = mw;
        }

        #endregion Konstruktori

        #region Metode

        /// <summary>
        ///  Korisnicka inicijalizacija i podesavanje OpenGL parametara.
        /// </summary>
        public void Initialize(OpenGL gl)
        {
            gl.ClearColor(0.8f,  0.9f,    0.95f, 0.5f);
            // Model sencenja na flat (konstantno)
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Enable(OpenGL.GL_CULL_FACE);
            SetupLighting(gl);
            SetupTexture(gl);
            gl.FrontFace(OpenGL.GL_CCW);
            m_scene.LoadScene();
            m_scene.Initialize();
            scaleBall = 20;
            rotateBall = 0;
            gore = true;
            dole = false;
            count = 0;
            zoom = 0;
            shooting = false;
            step = 1;
            main = null;
        }

        private void SetupTexture(OpenGL gl)
        {
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);

            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_NEAREST);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_NEAREST);

            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);

            gl.GenTextures(m_textureCount, m_textures);
            for (int i = 0; i < m_textureCount; ++i)
            {
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[i]);
                Bitmap image = new Bitmap(m_textureFiles[i]);
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                BitmapData imageData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, (int)OpenGL.GL_RGBA8, image.Width, image.Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR); 
                                                                                                                                                                                                                                                           // Linear Filtering 
                image.UnlockBits(imageData);
                image.Dispose();
            }

        }
        private void SetupLighting(OpenGL gl)
        {
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);

            float[] ambKomponenta = { 0.25f, 0.25f, 0.25f, 1.0f };
            float[] difKomponenta = { 0.75f, 0.75f, 0.75f, 1.0f };

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, ambKomponenta);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, difKomponenta);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, 180.0f);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, ambColor);
            gl.Enable(OpenGL.GL_LIGHT0);
            float[] pos = { 100f, 50f, -1000.0f, 1.0f };
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, pos);

            //svetlosni izvor iznad lopte
            float[] smer = { 0.0f, -1.0f, 0.0f };
            float[] plavaBoja = { 0f, 0.2f, 0.8f, 1f};
            double f = 2.5 * scaleBall - 160 + count;
            decimal dec = new decimal(f);
            float d = (float)dec;
            decimal dec1 = new decimal(zoom);
            float z = (float)dec1;
            decimal dec2 = new decimal(scaleBall);
            float scaleBall11 = (float)dec2;
            float[] pozicija = { 10.0f, d, -300f, 1.0f };
            //gl.Translate(-180f / 15 * step, (-20 - ((-191 + scaleBall / 2) + count)) / 15 * step, -430 / 15 * step);
            if (shooting)
            {
                float[] poz = { -180f / 15 * step, (-20 - ((-191 + scaleBall11 / 2) + count)) / 15 * step, -430 / 15 * step };
                pozicija[0] = pozicija[0] + poz[0];
                pozicija[1] = pozicija[1] + poz[1];
                pozicija[2] = pozicija[2] + poz[2];
            }
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, new float[] {1,1,1,1});
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_DIRECTION, smer);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_CUTOFF, 35.0f);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, plavaBoja);
            gl.Enable(OpenGL.GL_LIGHT1);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, pozicija);

        }

        internal void promeniBRotacije(double bRotacije)
        {
            rotateBall0 = bRotacije * 5;
        }

        internal void skalirajLoptu(double fSkaliranja)
        {
            scaleBall = fSkaliranja * scaleBall0;
        }
        internal void promeniAmbColor(float[] newAmbientColor)
        {
            ambColor = newAmbientColor;
        }

        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            if (shooting)
            {
                drawShooting(gl);
                return;
            }

            if (count >= 50)
            {
                dole = true;
                gore = false;
            }
            if (count <= 0)
            {
                gore = true;
                dole = false;
            }
            if (zoom >= 600)
                zoom = 600;
            if (zoom <= -300)
                zoom = -300;
            if (m_xRotation <= -20)
                m_xRotation = -17;
            else if (m_xRotation >= 50)
                m_xRotation = 50;
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.MatrixMode(OpenGL.GL_PROJECTION);      // selektuj Projection Matrix

            gl.LoadIdentity();
            gl.Perspective(50.0, (double)m_width / (double)m_height, 0.5, 1500.0);
            gl.LookAt(0f, 0f, 200f, 0f, 0f, -400, 0f, 1f, 0f);

            // Sacuvaj stanje modelview matrice i primeni transformacije
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.PushMatrix();

            gl.PushMatrix();

            
            gl.Translate(0, 0, zoom);
            
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);

            SetupLighting(gl);
            gl.PushMatrix();

            gl.Viewport(0, 0, m_width, m_height);

            gl.PushMatrix();

            //kamera

            #region PODLOGA
            gl.PushMatrix();
                    gl.Translate(0.0f, 0f, 0f);
                    gl.Color(0.42f, 0.81f, 0.67f);
            //da li treba ovde da se postavi za normalizaciju, ili u inicijalizaciji
                    gl.Enable(OpenGL.GL_NORMALIZE);

                    gl.MatrixMode(OpenGL.GL_TEXTURE);
                    gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Grass]);
                    gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);

                    gl.LoadIdentity();
                    gl.Scale(10, 10, 10);
                    gl.MatrixMode(OpenGL.GL_MODELVIEW);

                   gl.Begin(OpenGL.GL_QUADS);
                        gl.TexCoord(0.0f, 0.0f); gl.Vertex4f(600f, -200f, 1000, 1);
                        gl.TexCoord(0.0f, 1.0f); gl.Vertex4f(-600f, -200f, 1000, 1);
                        gl.TexCoord(1.0f, 1.0f); gl.Vertex4f(-600f, -200f, -1200, 1);
                        gl.TexCoord(1.0f, 0.0f); gl.Vertex4f(600f, -200f, -1200, 1);
                    gl.End();
                gl.PopMatrix();
            #endregion
            #region MODEL
            gl.PushMatrix();
                    gl.Translate(10.0f, (-191+scaleBall/2)+count, -300);
                    if (gore) { count += 3; }
                    if (dole) { count -= 3; }
                    rotateBall += rotateBall0;
                    gl.Rotate(rotateBall, 0, 1, 0);
                    gl.Color(1f, 1f, 1f);
                    gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Ball]);
                    gl.Scale(scaleBall, scaleBall, scaleBall);
                    m_scene.Draw();

            gl.PopMatrix();
            #endregion
            #region GOL
            //levo

            gl.PushMatrix();
                gl.Translate(-180f, -200f, -600);
                gl.Scale(5, 5, 5);
                gl.Rotate(90, 0.0f, 1.0f, 0.0f);
                gl.Rotate(-90, 1.0f, 0.0f, 0.0f);
                gl.Color(0.5f, 0.5f, 0.5f);

                Cylinder cil1 = new Cylinder();
                cil1.Slices = 50;
                cil1.Height = 40;
                cil1.BaseRadius = 2;
                cil1.TopRadius = 2;
                cil1.TextureCoords = true;
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastic]);
                gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
                cil1.CreateInContext(gl);
                cil1.Render(gl, RenderMode.Render);
           gl.PopMatrix();

                //desno
            gl.PushMatrix();
                gl.Translate(180f, -200, -600);
                gl.Scale(5, 5, 5);
                gl.Rotate(90, 0.0f, 1.0f, 0.0f);
                gl.Rotate(-90, 1.0f, 0.0f, 0.0f);
                gl.Color(0.5f, 0.5f, 0.5f);

                Cylinder cil2 = new Cylinder();
                cil2.Slices = 50;
                cil2.Height = 40;
                cil2.BaseRadius = 2;
                cil2.TopRadius = 2;
                cil2.TextureCoords = true;
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastic]);
                gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
                cil2.CreateInContext(gl);
                cil2.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            //gore
            gl.PushMatrix();
                gl.Translate(-192f, 0f, -600);
                gl.Scale(5, 5, 5);
                gl.Rotate(90, 0.0f, 1.0f, 0.0f);
                gl.Color(0.5f, 0.5f, 0.5f);

            Cylinder cil3 = new Cylinder();
                cil3.Height = 76.8;
                cil3.Slices = 50;
                cil3.BaseRadius = 2;
                cil3.TopRadius = 2;
                cil3.TextureCoords = true;
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastic]);
                gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
                cil3.CreateInContext(gl);
                cil3.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            //iza levo dole
            gl.PushMatrix();
                gl.Translate(-180f, -190f, -600);
                gl.Scale(5, 5, 5);
                gl.Rotate(180, 0.0f, 1.0f, 0.0f);
            gl.Color(0.5f, 0.5f, 0.5f);

            Cylinder cil4 = new Cylinder();
                cil4.Height = 20;
                cil4.Slices = 50;
                cil4.BaseRadius = 2;
                cil4.TopRadius = 2;
                cil4.TextureCoords = true;
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastic]);
                gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
                cil4.CreateInContext(gl);
                cil4.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            //iza desno dole
            gl.PushMatrix();
                gl.Translate(180f, -190f, -600);
                gl.Scale(5, 5, 5);
                gl.Rotate(180, 0.0f, 1.0f, 0.0f);
            gl.Color(0.5f, 0.5f, 0.5f);

            Cylinder cil5 = new Cylinder();
                cil5.Height = 20;
                cil5.Slices = 50;
                cil5.BaseRadius = 2;
                cil5.TopRadius = 2;
                cil5.TextureCoords = true;
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastic]);
                gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
                cil5.CreateInContext(gl);
                cil5.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            //iza dole
            gl.PushMatrix();
                gl.Translate(-190f, -195f, -700);
                gl.Scale(5, 5, 5);
                gl.Rotate(90, 0.0f, 1.0f, 0.0f);
            gl.Color(0.5f, 0.5f, 0.5f);

            Cylinder cil6 = new Cylinder();
                cil6.Height = 74.5;
                cil6.Slices = 50;
                cil6.BaseRadius = 1;
                cil6.TopRadius = 1;
                cil6.TextureCoords = true;
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastic]);
                gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
                cil6.CreateInContext(gl);
                cil6.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            //iza gore
            gl.PushMatrix();
                gl.Translate(-190f, -25f, -700);
                gl.Scale(5, 5, 5);
                gl.Rotate(90, 0.0f, 1.0f, 0.0f);
            gl.Color(0.5f, 0.5f, 0.5f);

            Cylinder cil11 = new Cylinder();
                cil11.Height = 74.5;
                cil11.Slices = 50;
                cil11.BaseRadius = 1;
                cil11.TopRadius = 1;
                cil11.TextureCoords = true;
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastic]);
                gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
                cil11.CreateInContext(gl);
                cil11.Render(gl, RenderMode.Render);
            gl.PopMatrix();
                //levo iza dole
                gl.PushMatrix();
                gl.Translate(-180f, -200f, -690);
                gl.Scale(5, 5, 5);
                gl.Rotate(90, 0.0f, 1.0f, 0.0f);
                gl.Rotate(-90, 1.0f, 0.0f, 0.0f);
            gl.Color(0.5f, 0.5f, 0.5f);

            Cylinder cil7 = new Cylinder();
                cil7.Slices = 50;
                cil7.Height = 35;
                cil7.BaseRadius = 1;
                cil7.TopRadius = 1;
                cil7.TextureCoords = true;
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastic]);
                gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
                cil7.CreateInContext(gl);
                cil7.Render(gl, RenderMode.Render);
                gl.PopMatrix();
                //desno iza dole
                gl.PushMatrix();
                gl.Translate(180f, -200f, -690);
                gl.Scale(5, 5, 5);
                gl.Rotate(90, 0.0f, 1.0f, 0.0f);
                gl.Rotate(-90, 1.0f, 0.0f, 0.0f);
            gl.Color(0.5f, 0.5f, 0.5f);

            Cylinder cil8 = new Cylinder();
                cil8.Slices = 50;
                cil8.Height = 35;
                cil8.BaseRadius = 1;
                cil8.TopRadius = 1;
                cil8.TextureCoords = true;
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastic]);
                gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
                cil8.CreateInContext(gl);
                cil8.Render(gl, RenderMode.Render);
                gl.PopMatrix();

                //desno iza gore
            gl.PushMatrix();
                gl.Translate(180f, 0, -600);
                gl.Scale(5, 5, 5);
                gl.Rotate(90, 0.0f, 1.0f, 0.0f);
                gl.Rotate(90, 1.0f, 0.0f, 0.0f);
                gl.Rotate(75, 0.0f, 1.0f, 0.0f);
                gl.Color(0.5f, 0.5f, 0.5f);

                 Cylinder cil9 = new Cylinder();
                cil9.Slices = 50;
                cil9.Height = 22;
                cil9.BaseRadius = 1;
                cil9.TopRadius = 1;
                cil9.TextureCoords = true;
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastic]);
                gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
                cil9.CreateInContext(gl);
                cil9.Render(gl, RenderMode.Render);
                gl.PopMatrix();

                //desno iza gore
                gl.PushMatrix();
                    gl.Translate(-180f, 0, -600);
                    gl.Scale(5, 5, 5);
                    gl.Rotate(90, 0.0f, 1.0f, 0.0f);
                    gl.Rotate(90, 1.0f, 0.0f, 0.0f);
                    gl.Rotate(75, 0.0f, 1.0f, 0.0f);
                    gl.Color(0.5f, 0.5f, 0.5f);

                    Cylinder cil10 = new Cylinder();
                    cil10.Slices = 50;
                    cil10.Height = 22;
                    cil10.BaseRadius = 1;
                    cil10.TopRadius = 1;
                    cil10.TextureCoords = true;
                    gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastic]);
                    gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
                    cil10.CreateInContext(gl);
                    cil10.Render(gl, RenderMode.Render);
                gl.PopMatrix();
            gl.PopMatrix();
            #endregion
            #region TEKST
            gl.PushMatrix();
                    gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
                    gl.Color(0, 0, 0);
                    gl.Viewport(m_width - 200, m_height - 150, 200, 150);
                    gl.MatrixMode(OpenGL.GL_PROJECTION);
                    gl.LoadIdentity();
                    gl.Ortho2D(-5.0, 10.0, 0, 6.0);
                    gl.MatrixMode(OpenGL.GL_MODELVIEW);
                    gl.LoadIdentity();
                    gl.PushMatrix();
                        gl.Color(0, 0, 0);
                        String[] tekst = { "Predmet: Racunarska grafika", "Sk.god: 2017/18", "Ime: Nina", "Prezime: Miladinovic", "Sifra zad: 9.2" };
                        for (int i = 0; i < tekst.Length; i++)
                        {
                            gl.PushMatrix();
                                gl.Translate(-5.0f, 5.0f - i, 0f);
                                gl.DrawText3D("Arial Bold", 10f, 1f, 0.1f, tekst[i]);
                            gl.PopMatrix();
                        }
                    gl.PopMatrix();
                gl.PopMatrix();

            #endregion

            gl.PopMatrix();
            gl.PopMatrix();
            gl.PopMatrix();

            gl.Flush();
        }
        #region ANIMATION
        private void drawShooting(OpenGL gl)
        {
            if (step == 15)
            {
                step = 1;
                shooting = false;
                gore = true;
                dole = false;
                count = 0;
                main.animationDone();
                main = null;
            }
            if (zoom >= 600)
                zoom = 600;
            if (zoom <= -300)
                zoom = -300;
            if (m_xRotation <= -20)
                m_xRotation = -17;
            else if (m_xRotation >= 50)
                m_xRotation = 50;

            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.MatrixMode(OpenGL.GL_PROJECTION);      // selektuj Projection Matrix

            gl.LoadIdentity();
            gl.Perspective(50.0, (double)m_width / (double)m_height, 0.5, 1500.0);
            gl.LookAt(0f, 0f, 200f, 0f, 0f, -600, 0f, 1f, 0f);

            // Sacuvaj stanje modelview matrice i primeni transformacije
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.PushMatrix();

            gl.PushMatrix();


            gl.Translate(0, 0, zoom);
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);
            gl.PushMatrix();

            SetupLighting(gl);

            gl.Viewport(0, 0, m_width, m_height);
            gl.PushMatrix();
        

            //kamera

            #region PODLOGA
            gl.PushMatrix();
            gl.Translate(0.0f, 0f, 0f);
            gl.Color(0.42f, 0.81f, 0.67f);
            //da li treba ovde da se postavi za normalizaciju, ili u inicijalizaciji
            gl.Enable(OpenGL.GL_NORMALIZE);

            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Grass]);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);

            gl.LoadIdentity();
            gl.Scale(10, 10, 10);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            gl.Begin(OpenGL.GL_QUADS);
            gl.TexCoord(0.0f, 0.0f); gl.Vertex4f(600f, -200f, 1000, 1);
            gl.TexCoord(0.0f, 1.0f); gl.Vertex4f(-600f, -200f, 1000, 1);
            gl.TexCoord(1.0f, 1.0f); gl.Vertex4f(-600f, -200f, -1200, 1);
            gl.TexCoord(1.0f, 0.0f); gl.Vertex4f(600f, -200f, -1200, 1);
            gl.End();
            gl.PopMatrix();
            #endregion
            #region MODEL
            gl.PushMatrix();
            gl.Translate(10.0f, (-191 + scaleBall / 2) + count, -300);
            gl.Translate(-180f/15*step, (-20-((-191 + scaleBall / 2) + count)) /15*step, -430 / 15 * step);
            step++;
            if (gore) { count += 3; }
            if (dole) { count -= 3; }
            rotateBall += rotateBall0;
            gl.Rotate(rotateBall, 0, 1, 0);
            gl.Color(1f, 1f, 1f);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Ball]);
            gl.Scale(scaleBall, scaleBall, scaleBall);
            m_scene.Draw();
            gl.PopMatrix();
            #endregion
            #region GOL
            //levo

            gl.PushMatrix();
            gl.Translate(-180f, -200f, -600);
            gl.Scale(5, 5, 5);
            gl.Rotate(90, 0.0f, 1.0f, 0.0f);
            gl.Rotate(-90, 1.0f, 0.0f, 0.0f);
            gl.Color(0.5f, 0.5f, 0.5f);

            Cylinder cil1 = new Cylinder();
            cil1.Slices = 50;
            cil1.Height = 40;
            cil1.BaseRadius = 2;
            cil1.TopRadius = 2;
            cil1.TextureCoords = true;
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastic]);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
            cil1.CreateInContext(gl);
            cil1.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            //desno
            gl.PushMatrix();
            gl.Translate(180f, -200, -600);
            gl.Scale(5, 5, 5);
            gl.Rotate(90, 0.0f, 1.0f, 0.0f);
            gl.Rotate(-90, 1.0f, 0.0f, 0.0f);
            gl.Color(0.5f, 0.5f, 0.5f);

            Cylinder cil2 = new Cylinder();
            cil2.Slices = 50;
            cil2.Height = 40;
            cil2.BaseRadius = 2;
            cil2.TopRadius = 2;
            cil2.TextureCoords = true;
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastic]);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
            cil2.CreateInContext(gl);
            cil2.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            //gore
            gl.PushMatrix();
            gl.Translate(-192f, 0f, -600);
            gl.Scale(5, 5, 5);
            gl.Rotate(90, 0.0f, 1.0f, 0.0f);
            gl.Color(0.5f, 0.5f, 0.5f);

            Cylinder cil3 = new Cylinder();
            cil3.Height = 76.8;
            cil3.Slices = 50;
            cil3.BaseRadius = 2;
            cil3.TopRadius = 2;
            cil3.TextureCoords = true;
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastic]);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
            cil3.CreateInContext(gl);
            cil3.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            //iza levo dole
            gl.PushMatrix();
            gl.Translate(-180f, -190f, -600);
            gl.Scale(5, 5, 5);
            gl.Rotate(180, 0.0f, 1.0f, 0.0f);
            gl.Color(0.5f, 0.5f, 0.5f);

            Cylinder cil4 = new Cylinder();
            cil4.Height = 20;
            cil4.Slices = 50;
            cil4.BaseRadius = 2;
            cil4.TopRadius = 2;
            cil4.TextureCoords = true;
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastic]);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
            cil4.CreateInContext(gl);
            cil4.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            //iza desno dole
            gl.PushMatrix();
            gl.Translate(180f, -190f, -600);
            gl.Scale(5, 5, 5);
            gl.Rotate(180, 0.0f, 1.0f, 0.0f);
            gl.Color(0.5f, 0.5f, 0.5f);

            Cylinder cil5 = new Cylinder();
            cil5.Height = 20;
            cil5.Slices = 50;
            cil5.BaseRadius = 2;
            cil5.TopRadius = 2;
            cil5.TextureCoords = true;
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastic]);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
            cil5.CreateInContext(gl);
            cil5.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            //iza dole
            gl.PushMatrix();
            gl.Translate(-190f, -195f, -700);
            gl.Scale(5, 5, 5);
            gl.Rotate(90, 0.0f, 1.0f, 0.0f);
            gl.Color(0.5f, 0.5f, 0.5f);

            Cylinder cil6 = new Cylinder();
            cil6.Height = 74.5;
            cil6.Slices = 50;
            cil6.BaseRadius = 1;
            cil6.TopRadius = 1;
            cil6.TextureCoords = true;
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastic]);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
            cil6.CreateInContext(gl);
            cil6.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            //iza gore
            gl.PushMatrix();
            gl.Translate(-190f, -25f, -700);
            gl.Scale(5, 5, 5);
            gl.Rotate(90, 0.0f, 1.0f, 0.0f);
            gl.Color(0.5f, 0.5f, 0.5f);

            Cylinder cil11 = new Cylinder();
            cil11.Height = 74.5;
            cil11.Slices = 50;
            cil11.BaseRadius = 1;
            cil11.TopRadius = 1;
            cil11.TextureCoords = true;
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastic]);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
            cil11.CreateInContext(gl);
            cil11.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            //levo iza dole
            gl.PushMatrix();
            gl.Translate(-180f, -200f, -690);
            gl.Scale(5, 5, 5);
            gl.Rotate(90, 0.0f, 1.0f, 0.0f);
            gl.Rotate(-90, 1.0f, 0.0f, 0.0f);
            gl.Color(0.5f, 0.5f, 0.5f);

            Cylinder cil7 = new Cylinder();
            cil7.Slices = 50;
            cil7.Height = 35;
            cil7.BaseRadius = 1;
            cil7.TopRadius = 1;
            cil7.TextureCoords = true;
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastic]);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
            cil7.CreateInContext(gl);
            cil7.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            //desno iza dole
            gl.PushMatrix();
            gl.Translate(180f, -200f, -690);
            gl.Scale(5, 5, 5);
            gl.Rotate(90, 0.0f, 1.0f, 0.0f);
            gl.Rotate(-90, 1.0f, 0.0f, 0.0f);
            gl.Color(0.5f, 0.5f, 0.5f);

            Cylinder cil8 = new Cylinder();
            cil8.Slices = 50;
            cil8.Height = 35;
            cil8.BaseRadius = 1;
            cil8.TopRadius = 1;
            cil8.TextureCoords = true;
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastic]);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
            cil8.CreateInContext(gl);
            cil8.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            //desno iza gore
            gl.PushMatrix();
            gl.Translate(180f, 0, -600);
            gl.Scale(5, 5, 5);
            gl.Rotate(90, 0.0f, 1.0f, 0.0f);
            gl.Rotate(90, 1.0f, 0.0f, 0.0f);
            gl.Rotate(75, 0.0f, 1.0f, 0.0f);
            gl.Color(0.5f, 0.5f, 0.5f);

            Cylinder cil9 = new Cylinder();
            cil9.Slices = 50;
            cil9.Height = 22;
            cil9.BaseRadius = 1;
            cil9.TopRadius = 1;
            cil9.TextureCoords = true;
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastic]);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
            cil9.CreateInContext(gl);
            cil9.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            //desno iza gore
            gl.PushMatrix();
            gl.Translate(-180f, 0, -600);
            gl.Scale(5, 5, 5);
            gl.Rotate(90, 0.0f, 1.0f, 0.0f);
            gl.Rotate(90, 1.0f, 0.0f, 0.0f);
            gl.Rotate(75, 0.0f, 1.0f, 0.0f);
            gl.Color(0.5f, 0.5f, 0.5f);

            Cylinder cil10 = new Cylinder();
            cil10.Slices = 50;
            cil10.Height = 22;
            cil10.BaseRadius = 1;
            cil10.TopRadius = 1;
            cil10.TextureCoords = true;
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastic]);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
            cil10.CreateInContext(gl);
            cil10.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            gl.PopMatrix();
            #endregion
            #region TEKST
            gl.PushMatrix();
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            gl.Color(0, 0, 0);
            gl.Viewport(m_width - 200, m_height - 150, 200, 150);
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Ortho2D(-5.0, 10.0, 0, 6.0);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();
            gl.PushMatrix();
            gl.Color(0, 0, 0);
            String[] tekst = { "Predmet: Racunarska grafika", "Sk.god: 2017/18", "Ime: Nina", "Prezime: Miladinovic", "Sifra zad: 9.2" };
            for (int i = 0; i < tekst.Length; i++)
            {
                gl.PushMatrix();
                gl.Translate(-5.0f, 5.0f - i, 0f);
                gl.DrawText3D("Arial Bold", 10f, 1f, 0.1f, tekst[i]);
                gl.PopMatrix();
            }
            gl.PopMatrix();
            gl.PopMatrix();

            #endregion
            gl.PopMatrix();
            gl.PopMatrix();
            gl.PopMatrix();
            gl.Flush();
        }

        #endregion

        /// <summary>
        /// Podesava viewport i projekciju za OpenGL kontrolu.
        /// </summary>
        public void Resize(OpenGL gl, int width, int height)
        {
            m_width = width;
            m_height = height;
            gl.Viewport(0, 0, width, height);
            gl.MatrixMode(OpenGL.GL_PROJECTION);      // selektuj Projection Matrix
            gl.LoadIdentity();
            //perspektiva
            gl.Perspective(50f, (double)width / height, 0.5f, 2000f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();                // resetuj ModelView Matrix
        }

        /// <summary>
        ///  Implementacija IDisposable interfejsa.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_scene.Dispose();
            }
        }

        #endregion Metode

        #region IDisposable metode

        /// <summary>
        ///  Dispose metoda.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable metode
    }
}
