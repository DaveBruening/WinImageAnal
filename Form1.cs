using Azure.AI.Vision.ImageAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
namespace WinImageAnal {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }
        IReadOnlyList<DenseCaption> idc;
        ImageAnalysisResult iar;
        private void button1_Click(object sender, EventArgs e) {
            if (openFileDialog1.ShowDialog() == DialogResult.OK) {
                string endpoint = Environment.GetEnvironmentVariable("VISION_ENDPOINT");
                string key = Environment.GetEnvironmentVariable("VISION_KEY");
                //pictureBox1.CreateGraphics()
                ImageAnalysisClient iac = new ImageAnalysisClient(new Uri(endpoint), new Azure.AzureKeyCredential(key));
                FileStream fs = new FileStream(openFileDialog1.FileName, FileMode.Open);
                BinaryData bd = BinaryData.FromStream(fs);
                fs.Close(); /*Before using Close(), got: The process cannot access the file ... 
                because it is being used by another process.*/
                VisualFeatures vf = VisualFeatures.DenseCaptions;
                iar = iac.Analyze(bd,vf);
                idc = iar.DenseCaptions.Values;
                //$"product:{iar.Metadata.Width*iar.Metadata.Height}";
                pictureBox1.Load(openFileDialog1.FileName);
            }
        }
        private void pictureBox1_Paint(object sender, PaintEventArgs e) {
            if (idc!=null) {
                textBox1.Text="";
                int cnt = 1;
                int lblTop = pictureBox1.Top;
                /*foreach (var ctrl in this.Controls)
                    textBox1.Text += ctrl.GetType().ToString() + ", "; */
                /*foreach(var ctrl in this.Controls.OfType<Label>())
                    //if (ctrl.GetType().ToString()=="System.Windows.Forms.Label")
                    this.Controls.Remove(ctrl); */
                var labels = this.Controls.OfType<Label>().ToList();
                foreach(Label label in labels) 
                    this.Controls.Remove(label);
                foreach (DenseCaption dc in idc) {
                    ImageBoundingBox ibb = dc.BoundingBox;
                    textBox1.Text += $"{dc.Text}   Left:{ibb.X}  Top:{ibb.Y}   " +
                        $"H:{ibb.Height}   W:{ibb.Width}   ratio:" +
                        $"{((ibb.Width*ibb.Height)/(iar.Metadata.Width*iar.Metadata.Height*.75)):F2}"
                        + Environment.NewLine;
                    if ((ibb.Width*ibb.Height) < (iar.Metadata.Width*iar.Metadata.Height*.75)) {
                        //Rectangle rct = new Rectangle(10,10,30,30);
                        Rectangle rct = new Rectangle(ibb.X, ibb.Y, ibb.Width, ibb.Height);
                        Label lbl = new Label();
                        lbl.Top = lblTop;
                        lbl.Left = pictureBox1.Left + pictureBox1.Width + 20;
                        lbl.Width = 200;
                        Pen pen;
                        if (cnt % 3 == 0) {
                            lbl.ForeColor = Color.Red;
                            pen = new Pen(Color.Red, 2); }
                        else if (cnt % 3 == 1) {
                            lbl.ForeColor = Color.Green;
                            pen = new Pen(Color.Green, 2); }
                        else { //if (cnt % 3 == 2)
                            lbl.ForeColor = Color.Blue;
                            pen = new Pen(Color.Blue, 2); }
                        e.Graphics.DrawRectangle(pen, rct);
                        lblTop += 30;
                        cnt++;
                        lbl.Text = dc.Text;
                        this.Controls.Add(lbl);
                    }
                }
                textBox1.Text += $"Image W:{iar.Metadata.Width}   H:{iar.Metadata.Height}   ";
            }
        }
    }
}
