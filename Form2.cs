using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace MandelbrotSetViewer
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private double X = 0; // initial center value
        private double Y = 0;
        private int W; // taille de la map
        private Bitmap mySmallBitmap ;
        private double form2_rc, form2_ic;
        private Form1 father;
        
        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void Form2_Shown(object sender, EventArgs e)
        {
            pictureBox1.Width = Math.Min(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Height = Math.Min(pictureBox1.Width, pictureBox1.Height);
            progressBar1.Maximum = pictureBox1.Height;
            progressBar1.Value = 0;
    }




        public void DrawSmallFractal(int n_max, double bande, double modulus_max, double c_r, double c_i, double w)
        {

            //MessageBox.Show("DEBUG *** n_max=" + n_max + " ; bande = " + bande + " ; modulus_max = " + modulus_max + " ; c_r=" + c_r + " ; c_i=" + c_i + " ; w=" +w);
            

            mySmallBitmap = new Bitmap(pictureBox1.Height, pictureBox1.Height);
            double x0 = X - w / 2;
            double y0 = Y + w / 2;
            
            int n;
            
            double z_r, z_i, i_temp, r_temp; ;
            int x, y;

            byte R;
            byte G;
            byte B;

            label1.Text = "c= "+Form1.ShowComplexNumber(c_r, c_i);
            form2_rc = c_r;
            form2_ic = c_i;

            
            W = pictureBox1.Height;
            progressBar1.Maximum = W; 
            progressBar1.Value = 0;


            for (x = 0; x < W; x++)
            {
                for (y = 0; y < W; y++)
                {

                    
                    /*************
                    * Julia Set *
                    *************/

                    // Pn+1(z) = z²+c
                    // c est donné (fixe)
                    // on fait varier z0 sur le plan complexe

                    z_r = x0 + x * w / W; // initialization of z
                    z_i = y0 - y * w / W;

                    n = 0;
                    do
                    {
                        r_temp = Math.Pow(z_r, 2) - Math.Pow(z_i, 2) + c_r;
                        i_temp = 2 * z_r * z_i + c_i;
                        z_r = r_temp;
                        z_i = i_temp;
                        n++;
                    } while ((n < n_max) && (Form1.modulus(z_r, z_i) < modulus_max));


                    // Draw pixel   

                    R = Form1.gauss_color(n, n_max, bande, 1);
                    G = Form1.gauss_color(n, n_max, bande, 2);
                    B = Form1.gauss_color(n, n_max, bande, 3);

                    mySmallBitmap.SetPixel(x, y, Color.FromArgb(R, G, B));
                                      

                }

                progressBar1.Value++;
                this.Refresh();

            }

            progressBar1.Value = 0;

            pictureBox1.Image = mySmallBitmap;
            this.Refresh();
        }

        public void SetCallerReference (Form1 f)
        {
            father = f;
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            father.setRefToSmallFormtoNULL();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked) // new instance
            {
                
                Process newInstance = new Process();
                newInstance.StartInfo.FileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                newInstance.StartInfo.Arguments = "J " + form2_rc + " " + form2_ic;
                newInstance.Start();


            } else
            {
                father.DrawJFromOutside(form2_rc,form2_ic);
                this.Close();
            }

            

        }
    }
}
