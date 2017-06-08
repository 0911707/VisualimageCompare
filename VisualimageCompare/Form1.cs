using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace VisualimageCompare{
    public partial class Form1 : Form{
        public Bitmap baseImg;
        public Bitmap testImg;
        public Bitmap newImg;
        public Bitmap staticImg;
        int correctPixels = 0;
        int totalCountedPixels = 0;
        string baseImgText;
        string testImgText;
        string[] fileNames;
        private PictureBox newPictureBox;

        public Form1(){
            InitializeComponent();
            fileNames = Directory.GetFiles("C:\\Users\\Julian\\Pictures\\testmap");
            pictureBox1.Image = baseImg;
            pictureBox2.Image = testImg;
            staticImg = baseImg;
            testImgText = "";
            baseImgText = "";
        }

        //START LOGIC

        //check if the colors match close enough to compensate for compression errors
        public bool almostEquals(Color basePixel, Color testPixel, int margin){
            bool alpha = Math.Abs(basePixel.A - testPixel.A) <= margin;
            bool red = Math.Abs(basePixel.R - testPixel.R) <= margin;
            bool green = Math.Abs(basePixel.G - testPixel.G) <= margin;
            bool blue = Math.Abs(basePixel.B - testPixel.B) <= margin;
            if (alpha && red && green && blue){
                return true; //same (within margin)
            }
            else{
                return false; //different
            }
        }

        //main comparison function
        public void loopOverPictures(Bitmap baseImg1, Bitmap testImg1){
            Bitmap baseImg_ = baseImg1;
            Bitmap testImg_ = testImg1;
            bool altered = false;
            int x = 0;
            Color pixel = Color.FromName("yellow");

            //create new imagebox to draw in
            newPictureBox = new PictureBox();
            this.newPictureBox.Location = new System.Drawing.Point(10, 55);

            //make a copy of the baseImg so this one can be edited instead
            newImg = new Bitmap(baseImgText);

            pictureBox1.Image = newImg;
            newPictureBox.Update();

           
            //check if image needs to be resized
            if (!(baseImg_.Width == testImg_.Width) && !(baseImg_.Height == testImg_.Height)){
                testImg_ = resizeImage(testImg_, baseImg_.Width, baseImg_.Height);
            }
            int tempCnt = 0;
            //loop over all pixels in the image
            while (x < baseImg_.Width){ // TODO replace x and y with variable adjustable via GUI ????
                int y = 0;
                while (y < baseImg_.Height){
                    Color basePixel = baseImg_.GetPixel(x, y);
                    Color testPixel = testImg_.GetPixel(x, y);

                    if (almostEquals(basePixel, testPixel, 10)){ // TODO replace 10 with variable margin adjustable via GUI
                        correctPixels++;
                    }
                    else{
                        newImg.SetPixel(x, y, pixel);
                        altered = true;
                    }
                    totalCountedPixels++;
                    y += 1;
                }

                tempCnt++;
                if (tempCnt >= 25 && altered){
     
                    newPictureBox.Image = newImg;
                    newPictureBox.Update();
                    altered = false;              
                    tempCnt = 0;
                }
                x += 1;
            }
        }

        //resize image to baseImg dimensions and return a bitmap of new img
        public Bitmap resizeImage(Image image, int width, int height){
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using (var graphics = Graphics.FromImage(destImage)){
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes()){
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }

        // todo improve this
        public void loopOverFolderPictures(){
            foreach (string file in fileNames){
                testImgText = file;
                testImg = new Bitmap(testImgText);
                pictureBox2.Image = testImg;
                pictureBox2.Update();
                loopOverPictures(baseImg, testImg);
                pictureBox1.Image = staticImg;
                pictureBox1.Update();
            }
        }

        public void showCurrentColumn(){
            int height = testImg.Height;
        }

        //END LOGIC

        //START CONTROLS
        private void runComparison(){
            loopOverPictures(baseImg, testImg);
        }

        //button to start comparing only the 2 selected images
        private void button1_Click(object sender, EventArgs e){
            if ( baseImgText != "" && testImgText != "" ){ // check if images have been chosen
                loopOverPictures(baseImg, testImg);
                double percentage = ((Convert.ToDouble(correctPixels) / Convert.ToDouble(totalCountedPixels)) * 100);
                button1.Text = percentage.ToString() + "% equal";
            }
        }

        //select baseImg
        public void button2_Click(object sender, EventArgs e){
            OpenFileDialog file = new OpenFileDialog();
            if (file.ShowDialog() == DialogResult.OK){
                baseImgText = file.FileName;
                baseImg = new Bitmap(baseImgText);
                pictureBox1.Image = baseImg;
                pictureBox1.Update();
                button1.Text = "Compare";
            }
            baseImgName.Text = file.FileName;
        }

        //select testImg
        private void button3_Click(object sender, EventArgs e){
            OpenFileDialog file = new OpenFileDialog();
            if(file.ShowDialog() == DialogResult.OK){
                testImgText = file.FileName;
                testImg = new Bitmap(testImgText);
                pictureBox2.Image = testImg;
                pictureBox2.Update();
                button1.Text = "Compare";
            }
            testImgName.Text = file.FileName;
        }

        //button to select folder to compare baseImg too
        private void button4_Click(object sender, EventArgs e){
            //Open folder selection and put all files in that folder to an array
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = "C:\\Users\\Julian\\Pictures";
            if (fbd.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath)){
                string[] files = Directory.GetFiles(fbd.SelectedPath);
                MessageBox.Show("Files found: " + files.Length.ToString());
                fileNames = Directory.GetFiles(fbd.SelectedPath);
            }
            else{
                MessageBox.Show("Please select a valid map", "IMPORTANT !!!");
            }

            //loop thru all images in folder and compare them to base image
            foreach (string file in fileNames){
                testImgText = file;
                testImg = new Bitmap(testImgText);
                pictureBox2.Image = testImg;
                pictureBox2.Update();
                loopOverPictures(baseImg, testImg);
                pictureBox1.Image = baseImg;
                pictureBox1.Update();
            }
            results results = new results();
            results.Show();
        }

        private void Form1_Load(object sender, EventArgs e){
            
        }
        //END CONTROLS
    }
}