using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;


namespace MandelbrotSetViewer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public static double defaultWforJ = 3;
        public static double defaultWforM = 4;

        private double X = 0; // initial center value
        private double Y = 0;
        private double w = defaultWforM; // largeur logique
        private int W; // taille de la map

        public double modulus_max = Math.Exp(1000);

        private static int NBThreads = 20;
        private Thread[] MyThreads;
        String datetimeformat = "dd-MM-yyyy_HH-mm-ss";
        Bitmap myBitmap;

        private DateTime start_time, end_time;
        private string duration;

        private byte[][,] myRGB_R = new byte[NBThreads][,];
        private byte[][,] myRGB_G = new byte[NBThreads][,];
        private byte[][,] myRGB_B = new byte[NBThreads][,];

        List<double[]> predefinedList_c = new List<double[]>();

        bool DrawM = true;

        bool monocolor;

        double bande;
        int n_max;

        static String dformat = "0.0000000";

        private Form2 smallForm;

        enum txtAlign
        {
            TopLeft,
            BottomRight,
            BottomLeft
        }

        // for launching J in command line
        private bool blaunchJDrawingWhenStarting = false;
        private double local_r, local_i;


        public static double modulus(double a, double b)
        {
            // return a modulus of a complex = SQRT (real²+im²)
            return Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
        }

        public static byte gauss_color(int n, int n_max, double bande, int coef)
        {
            /*   
             gaussienne autour de n_max / 4; n_max*2/4 et n_max*3/4 
             et ensuite 255 * la gaussienne 
             */

            double col;
            if (coef == 1)
            {
                col = 255 * Math.Exp(-Math.Pow((n - n_max * coef / 4), 2) / (n_max / bande));
            }
            else
            {
                col = 255 * Math.Exp(-Math.Pow((n - n_max * coef / 4) / (n_max / bande), 2));
            }
            return (byte)Math.Truncate(col);

        }



        private void DrawFractal(Object i_thread)
        {
            W = pictureBox1.Width; // = pictureBox2.Height
            double x0 = X - w / 2;
            double y0 = Y + w / 2;
            bande = Convert.ToDouble(textBox2.Text);
            int n;
            n_max = Convert.ToInt32(textBox1.Text);
            int n_min = Convert.ToInt32(textBox3.Text);



            double c_r, c_i, z_r, z_i, i_temp, r_temp; ;
            int x, y;

            byte R;
            byte G;
            byte B;

            int borne_inf = (int)i_thread * W / NBThreads;
            int borne_sup = ((int)i_thread + 1) * W / NBThreads;

            c_r = Convert.ToDouble(textBox4.Text); // for Julia drawing
            c_i = Convert.ToDouble(textBox5.Text);


            for (x = borne_inf; x < borne_sup; x++)
            {
                for (y = 0; y < W; y++)
                {

                    if (DrawM)
                    {

                        /**************
                         * Mandelbrot *
                         **************/

                        // Pn+1(z) = z²+c
                        // z0=0, on fait varier c sur le plan complexe

                        z_r = 0;
                        z_i = 0;
                        c_r = x0 + x * w / W; // initialization of z
                        c_i = y0 - y * w / W;

                    }
                    else
                    {
                        /*************
                         * Julia Set *
                         *************/

                        // Pn+1(z) = z²+c
                        // c est donné (fixe)
                        // on fait varier z0 sur le plan complexe

                        z_r = x0 + x * w / W; // initialization of z
                        z_i = y0 - y * w / W;

                    }

                    n = 0;
                    do
                    {
                        r_temp = Math.Pow(z_r, 2) - Math.Pow(z_i, 2) + c_r;
                        i_temp = 2 * z_r * z_i + c_i;
                        z_r = r_temp;
                        z_i = i_temp;
                        n++;
                    } while ((n < n_max) && (modulus(z_r, z_i) < modulus_max));



                    /************************
                     * Draw pixel (M or J)  *
                     ************************/


                    if (monocolor) // monochrome
                    {
                        if ((n >= n_min) && (n < n_max))
                        {
                            myRGB_R[(int)i_thread][x - borne_inf, y] = 254;
                            myRGB_G[(int)i_thread][x - borne_inf, y] = 0;
                            myRGB_B[(int)i_thread][x - borne_inf, y] = 0;
                            //myBitmap.SetPixel(x, y, Color.FromArgb(255, 0, 0));
                        }
                        else
                        {
                            myRGB_R[(int)i_thread][x - borne_inf, y] = 240;
                            myRGB_G[(int)i_thread][x - borne_inf, y] = 240;
                            myRGB_B[(int)i_thread][x - borne_inf, y] = 240;
                        }


                    }
                    else // dégradé RGB
                    {
                        R = gauss_color(n, n_max, bande, 1);
                        G = gauss_color(n, n_max, bande, 2);
                        B = gauss_color(n, n_max, bande, 3);

                        try
                        {
                            myRGB_R[(int)i_thread][x - borne_inf, y] = R;
                            myRGB_G[(int)i_thread][x - borne_inf, y] = G;
                            myRGB_B[(int)i_thread][x - borne_inf, y] = B;
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("x-borne_inf=" + (x - borne_inf) + ";y=" + y);
                            throw e;
                        }



                        //myBitmap.SetPixel(x, y, Color.FromArgb(R, G, B));

                    }

                }

                IncreaseProgressbar(i_thread);

            }


        }


        private void ShowBitMap()
        {
            byte R, G, B;
            int w = pictureBox1.Width / NBThreads;

            for (int n = 0; n < NBThreads; n++)
            {
                for (int x = 0; x < w; x++)
                {

                    for (int y = 0; y < pictureBox1.Width; y++)
                    {

                        R = myRGB_R[n][x, y];
                        G = myRGB_G[n][x, y];
                        B = myRGB_B[n][x, y];

                        myBitmap.SetPixel(n * w + x, y, Color.FromArgb(R, G, B));

                    }

                }


            }

        }



        private void Form1_Shown(object sender, EventArgs e)
        {
            // default values
            textBox1.Text = "250";
            textBox2.Text = "4.5";
            textBox3.Text = "30";
            pictureBox1.Width = Math.Min(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Height = Math.Min(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Enabled = false;
            label3.Enabled = false;
            textBox3.Enabled = false;
            myBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Width);

            W = pictureBox1.Width;
            int w = pictureBox1.Width / NBThreads;

            //MessageBox.Show("pictureBox1.Width / NBThreads=" + pictureBox1.Width + "/" + NBThreads);

            pictureBox1.Width = w * NBThreads; // pour eviter les pb d'arrondi
            pictureBox1.Height = w * NBThreads;

            //MessageBox.Show("w,h=" + w + ";" + pictureBox1.Height);
            //MessageBox.Show("pictureBox1.Width / NBThreads=" + pictureBox1.Width + "/" + NBThreads);

            for (int i = 0; i < NBThreads; i++)
            {
                myRGB_R[i] = new byte[w, pictureBox1.Height];
                myRGB_G[i] = new byte[w, pictureBox1.Height];
                myRGB_B[i] = new byte[w, pictureBox1.Height];
            }


            initMWindow();

            InitializePredefinedList();

            JuliaOrMControlsCheckedStatus();

            // MessageBox.Show("datetime=" + DateTime.Now.ToString(datetimeformat));

            if (blaunchJDrawingWhenStarting)
            {
                DrawJFromOutside(local_r, local_i);
            }



        }

        private void button1_Click(object sender, EventArgs e)
        {

            
            pictureBox1.Enabled = true;
            if (checkBox2.Checked)
            {
                initMWindow();
            }

            initProgressBar();

            DrawMInMultiThread();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            W = pictureBox1.Width;
            int relative_x = MousePosition.X - pictureBox1.Left - this.Left - 8;
            int relative_y = MousePosition.Y - pictureBox1.Top - this.Top - 30;
            //MessageBox.Show("MousePositionY=" + MousePosition.Y + " ; pictureBox2.Top=" + pictureBox2.Top+ " ; this.Top=" + this.Top);
            //MessageBox.Show("RelativeX=" + relative_x + "RelativeY=" + relative_y);

            //double new_x = (relative_x - W / 2) * w / W;
            //double new_y = (W / 2 - relative_y) * w / W;

            double new_x = X - w * (0.5 - (double)relative_x / W);
            double new_y = Y + w * (0.5 - (double)relative_y / W);


            if (!checkBox5.Checked)
            {
                /*
                Pen pen = new Pen(Color.BlueViolet);
                Graphics g = pictureBox1.CreateGraphics();
                g.DrawRectangle(pen, relative_x - W / 8, relative_y - W / 8, W / 4, W / 4);
                */

                X = new_x;
                Y = new_y;
                //MessageBox.Show("new X,Y=" + X + "," + Y);

                Graphics gr = Graphics.FromImage(myBitmap);
                //Pen pen = new Pen(Color.BlueViolet);
                Rectangle rect = new Rectangle(relative_x - W / 8, relative_y - W / 8, W / 4, W / 4);
                //gr.DrawRectangle(pen, rect);
                SolidBrush semiTransBrush = new SolidBrush(Color.FromArgb(70, 255, 0, 0));
                //gr.CompositingQuality = CompositingQuality.GammaCorrected;
                gr.FillRectangle(semiTransBrush, rect);


                w = w / 4;

                int n_min = Convert.ToInt32(textBox3.Text);
                int n_max = Convert.ToInt32(textBox1.Text);
                n_min = (n_max + 19 * n_min) / 20;
                textBox3.Text = Convert.ToString(n_min);

                initProgressBar();

                checkBox2.Checked = false;

                DrawMInMultiThread();
            } else
            {

                if (smallForm == null)
                {
                    smallForm = new Form2();
                    smallForm.Show();
                    smallForm.SetCallerReference(this);
                } else
                {
                    smallForm.BringToFront();
                }

                smallForm.DrawSmallFractal(n_max, bande, modulus_max, new_x, new_y, defaultWforJ);


            }




        }

        private void initMWindow()
        {
            X = 0; // initial center value
            Y = 0;
            if (DrawM)
            {
                w = defaultWforM; // largeur logique
            } else
            {
                w = defaultWforJ; // largeur logique
            }
                

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            label3.Enabled = checkBox1.Checked;
            textBox3.Enabled = checkBox1.Checked;
        }

        private void initProgressBar()
        {
            progressBar1.Minimum = 0;
            progressBar1.Maximum = pictureBox1.Width;
            progressBar1.Value = 0;

            label11.Text = "w = " + w.ToString();

            Cursor = Cursors.WaitCursor;

        }

        private void IncreaseProgressbar(Object n)
        {
            if (progressBar1.InvokeRequired)
            {
                // MessageBox.Show("progressBar1.InvokeRequired" + n);
                Action safeAddValuetoProgressbar = delegate { IncreaseProgressbar(n); };
                progressBar1.Invoke(safeAddValuetoProgressbar);
            }
            else
            {
                //MessageBox.Show("progressBar1.Value++ " + n);
                if (progressBar1.Value < progressBar1.Maximum)
                {
                    progressBar1.Value++;
                    this.Refresh();
                }
            }
        }

        private void DrawMInMultiThread()
        {

            monocolor = checkBox1.Checked;

            start_time = DateTime.Now;

            Thread backgroundThread = new Thread(() =>
            {

                MyThreads = new Thread[NBThreads];
                for (int i = 0; i < NBThreads; i++)
                {
                    MyThreads[i] = new Thread(DrawFractal);
                    MyThreads[i].Start(i);
                }



                for (int i = 0; i < MyThreads.Length; i++)
                {
                    MyThreads[i].Join();
                }


                end_time = DateTime.Now;
                duration = (end_time - start_time).ToString("ss");

                ShowBitMap();


                AfterDrawFinished(null);

                ShowDuration(null);


            });

            backgroundThread.Start();


        }


        private void AfterDrawFinished(Object n)
        {



            if (pictureBox1.InvokeRequired)
            {
                // MessageBox.Show("progressBar1.InvokeRequired" + n);
                Action safeDrawMinpictureBox = delegate { AfterDrawFinished(n); };
                pictureBox1.Invoke(safeDrawMinpictureBox);
            }
            else
            {
                String filename = DateTime.Now.ToString(datetimeformat) + "_d" + duration;

                ShowDetailsOnTheMap();


                pictureBox1.Image = myBitmap;
                myBitmap.Save("bitmap_" + filename + ".jpg");
                progressBar1.Value = 0;

                Cursor = Cursors.Default;

            }



        }

        private void ShowDuration(Object n)
        {


            if (label5.InvokeRequired)
            {
                // MessageBox.Show("progressBar1.InvokeRequired" + n);
                Action safeShowDuration = delegate { ShowDuration(n); };
                pictureBox1.Invoke(safeShowDuration);
            }
            else
            {
                label5.Text = duration;
            }



        }

        private void ShowDetailsOnTheMap()
        {

            if (checkBox3.Checked)
            {
                String coordinate1 = "(" + (X - w / 2).ToString(dformat) + "," + (Y + w / 2).ToString(dformat) + ")";
                String coordinate2 = "(" + (X + w / 2).ToString(dformat) + "," + (Y - w / 2).ToString(dformat) + ")"; 
                ShowSomethingOntheMap (coordinate1, txtAlign.TopLeft);
                ShowSomethingOntheMap (coordinate2, txtAlign.BottomRight);
            }

            // show c in Julia image
            if ((!DrawM) && (checkBox4.Checked))
            {
                double c_r = Convert.ToDouble(textBox4.Text);
                double c_i = Convert.ToDouble(textBox5.Text);
                String complexAsString = "c = " + ShowComplexNumber(c_r, c_i);
                ShowSomethingOntheMap (complexAsString, txtAlign.BottomLeft);
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            int indx = comboBox1.SelectedIndex;
            if (indx > -1)
            {
                textBox4.Text = predefinedList_c.ElementAt(indx)[0].ToString();
                textBox5.Text = predefinedList_c.ElementAt(indx)[1].ToString();
            }


        }

        private void InitializePredefinedList()
        {

            predefinedList_c.Add(new double[2] { -0.7927, 0.1609 });
            predefinedList_c.Add(new double[2] { -0.1134, 0.8606 });
            predefinedList_c.Add(new double[2] { -1.1380, 0.2403 });
            predefinedList_c.Add(new double[2] { -0.1225, 0.7449 });
            predefinedList_c.Add(new double[2] { 0.32, 0.043 });
            predefinedList_c.Add(new double[2] { -0.0986, -0.65186 });
            predefinedList_c.Add(new double[2] { -0.3380, -0.6230 });
            predefinedList_c.Add(new double[2] { -0.7707, 0.1 });
            predefinedList_c.Add(new double[2] { -0.7927, 0.141 });
            predefinedList_c.Add(new double[2] { -0.338, -0.611 });
            predefinedList_c.Add(new double[2] { -0.0986, -0.6491 });

            comboBox1.Items.Clear();
            for (int i = 0; i < predefinedList_c.Count; i++)
            {
                double re_c, im_c;
                re_c = predefinedList_c.ElementAt(i)[0];
                im_c = predefinedList_c.ElementAt(i)[1];
                if (im_c > 0)
                {
                    comboBox1.Items.Add(re_c.ToString() + " + " + im_c.ToString() + " i");
                } else
                {
                    comboBox1.Items.Add(re_c.ToString() + " " + im_c.ToString() + " i");
                }



            }


        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            checkBox2.Checked = true;
            JuliaOrMControlsCheckedStatus();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            checkBox2.Checked = true;
            JuliaOrMControlsCheckedStatus();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {


            int relative_x = MousePosition.X - pictureBox1.Left - this.Left - 8;
            int relative_y = MousePosition.Y - pictureBox1.Top - this.Top - 30;

            double new_x = X - w * (0.5 - (double)relative_x / W);
            double new_y = Y + w * (0.5 - (double)relative_y / W);

            label10.Text = ShowComplexNumber(new_x, new_y);

        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            label10.Text = "null";
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            /*Graphics g = Graphics.FromImage(myBitmap);
            Pen p = new Pen(Color.White);
            Point p1 = new Point(0,pictureBox1.Height/2);
            Point p2 = new Point(pictureBox1.Height, pictureBox1.Height / 2);
            g.DrawLine(p, p1, p2);
            //MessageBox.Show("p1="+ 0 +","+ (pictureBox1.Height / 2));
            p1 = new Point(pictureBox1.Height / 2,0);
            p2 = new Point(pictureBox1.Height / 2, pictureBox1.Height);
            g.DrawLine(p, p1, p2);
            pictureBox1.Image = myBitmap;
            */


        }

        private void JuliaOrMControlsCheckedStatus()
        {
            label6.Enabled = radioButton2.Checked;
            label7.Enabled = radioButton2.Checked;
            label8.Enabled = radioButton2.Checked;
            textBox4.Enabled = radioButton2.Checked;
            textBox5.Enabled = radioButton2.Checked;
            comboBox1.Enabled = radioButton2.Checked;
            button2.Enabled = radioButton2.Checked;
            checkBox4.Enabled = radioButton2.Checked;
            groupBox3.Enabled = radioButton1.Checked;
            if (radioButton2.Checked)
            {
                button1.Text = "Draw J !";
                checkBox5.Checked = false;
                DrawM = false;
            } else
            {
                button1.Text = "Draw M !";
                DrawM = true;
            }

        }


        public static String ShowComplexNumber(double r, double i)
        {

            return "(" + r.ToString(dformat) + " , " + i.ToString(dformat) +")";

            /*if (i < 0)
            {
                return r.ToString(dformat) + " - " + (-1 * i).ToString(dformat)+" i";
            } else if (i == 0)
            {
                return r.ToString(dformat);
            } else
            {
                return r.ToString(dformat) + " + " + i.ToString(dformat)+" i";
            }
            */
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {

        }

        public void DrawJFromOutside(double rc, double ic)
        {

            // launch drawing J from outside the form (command line or from another form)

            pictureBox1.Enabled = true;
            radioButton2.Checked = true;
            initMWindow();
            initProgressBar();

            textBox4.Text = rc.ToString();
            textBox5.Text = ic.ToString();
            checkBox4.Checked = true;

            DrawMInMultiThread();

        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {

        }

        public void setRefToSmallFormtoNULL()
        {
            smallForm = null;
        }

        private void ShowSomethingOntheMap(String text, txtAlign txtalign)
        {
            if (checkBox1.Checked)
            {
                return; 
            }
            
            Graphics g = Graphics.FromImage(myBitmap);
            StringFormat stringFormat = new StringFormat();
            stringFormat.LineAlignment = StringAlignment.Near;
            Rectangle rect;

            switch (txtalign)
            {
                case txtAlign.TopLeft:
                    rect = new Rectangle(5, 5, 400, 400);
                    stringFormat.Alignment = StringAlignment.Near;
                    break;

                case txtAlign.BottomRight:
                    rect = new Rectangle(pictureBox1.Width - 405, pictureBox1.Width - 15, 400, 400);
                    stringFormat.Alignment = StringAlignment.Far;
                    break;

                case txtAlign.BottomLeft:
                    rect = new Rectangle(5, pictureBox1.Width - 15, 400, 400);
                    stringFormat.Alignment = StringAlignment.Near;
                    break;

                default:
                    rect = new Rectangle(5, 5, 400, 400);
                    stringFormat.Alignment = StringAlignment.Near;
                    break;
            }

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            
            g.DrawString(text, new Font("Tahoma", 8), Brushes.White, rect, stringFormat);



        }

        public void TestArgs (string[] args)
        {
            MessageBox.Show("testing command line arguments ");
            for (int i = 0; i < args.Length; i++)
            {
                MessageBox.Show("args[{" + i+"}] == {" + args[i]  + "}");
                
            }


        }

        public void LaunchJDrawingWhenStarting (double [] cplx)
        {
            blaunchJDrawingWhenStarting = false;

            if (cplx == null)
            {
                return;
            }

            if (cplx.Length != 2)
            {
                return;
            }

            blaunchJDrawingWhenStarting = true;
            local_r = cplx[0];
            local_i = cplx[1];
        }

        public static double[] ExtractCFromArgs (String[] args)
        {
            
            if (args.Length !=3)
            {
                Console.WriteLine("bad arguments - expecting 3 arguments ..");
                return null;
            }

            if (! args[0].Equals("J") )
            {
                Console.WriteLine("bad arguments - expecting J as first argument.");
                return null;
            }

            return new double[2] { Convert.ToDouble(args[1]), Convert.ToDouble(args[2]) };


        }

    }




}
