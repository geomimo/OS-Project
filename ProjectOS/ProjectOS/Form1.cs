using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjectOS
{
    public partial class Form1 : Form
    {
        Page[] disk;
        Page[] memory;
        int[] sequence;
        int totalFaults, currentFaults, currentTime, pagesInMem;
        string text;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Location = new Point(0, 0);
            this.Height = Screen.PrimaryScreen.Bounds.Height - 50;
            richTextBox1.Size = new Size(this.Width - 100, this.Height - 100);
        }

        private void GenerateSeq()
        {  
            Random r = new Random();
            text += Environment.NewLine + "SEQUENCE: ";
            for(int i = 0; i < 20; i++)
            {
                int rIndex;
                if (r.Next(100) < 60 && pagesInMem > 19)
                {
                    do
                    {
                        rIndex = r.Next(pagesInMem);
                    } while (sequence.Contains(rIndex));
                    sequence[i] = memory[rIndex].ID;
                }
                else
                {
                    do
                    {
                        rIndex = r.Next(128);
                    }while (memory.Take(pagesInMem).Select(x=>x.ID).Contains(disk[rIndex].ID) || sequence.Contains(disk[rIndex].ID));
                    
                    sequence[i] = disk[rIndex].ID;
                }
                text += sequence[i].ToString() + " ";
            }
            text += Environment.NewLine;
        }

        private Page FindMin()
        {
            int min = Int32.MaxValue;
            Page minPage = null;
            for(int i = 0; i < pagesInMem; i++)
            {
                int counter = sequence.Contains(memory[i].ID) && memory[i].Counter[0] ? 128 : 0;
                for (int j = 0; j < 8; j++)
                {
                    counter += memory[i].Counter[j] ? (int)Math.Pow(2D, (double)(7 - j)) : 0;
                }
                if (min > counter)
                {
                    min = counter;
                    minPage = memory[i];
                }
            }
            return minPage;
        }

        private void StartBtn_Click(object sender, EventArgs e)
        {
            totalFaults = 0;
            currentTime = 0;
            pagesInMem = 0;
            text = "";
            progressBar1.Value = 0;
            label1.Text = "TOTAL FAULTS: ";

            disk = new Page[128];
            memory = new Page[32];
            sequence = new int[20];
            Page.Fill(disk);
            Random r = new Random();

            Thread.Sleep(500);

            for (int i = 0; i < 500; i++)
            {          
                progressBar1.PerformStep();
                GenerateSeq();
                currentFaults = 0;
                for(int j = 0; j < pagesInMem; j++)
                {
                    memory[j].Reference = false;
                    memory[j].ShiftCounter();
                }

                for (int j = 0; j < 20; j++)
                {
                    Page callingPage = disk[sequence[j]].Copy();
                    if (callingPage.Position != -1)
                    {
                        memory[callingPage.Position].Reference = true;
                        memory[callingPage.Position].ShiftCounter();
                        memory[callingPage.Position].Counter[0] = true;
                        memory[callingPage.Position].LastReferenceTime = currentTime;
                    }
                    else
                    {
                        int indexMem;
                        if (pagesInMem < 32)
                        {
                            indexMem = pagesInMem++;
                        }
                        else
                        {
                            indexMem = FindMin().Position;
                            int indexDisk = memory[indexMem].ID;
                            disk[indexDisk] = memory[indexMem].Copy();
                            disk[indexDisk].Reference = false;
                            disk[indexDisk].Counter.SetAll(false);
                            disk[indexDisk].Position = -1;
                        }

                        memory[indexMem] = callingPage.Copy();
                        memory[indexMem].Reference = true;
                        memory[indexMem].ShiftCounter();
                        memory[indexMem].Counter[0] = true;
                        memory[indexMem].LoadTime = currentTime;
                        memory[indexMem].LastReferenceTime = currentTime;
                        memory[indexMem].Position = indexMem;
                        disk[callingPage.ID].Position = indexMem;
                        totalFaults++;
                        currentFaults++;
                    }
                }

                #region display
                for (int j = 0; j < pagesInMem; j++)
                {
                    string bitCounter = "";

                    for (int k = 0; k < 8; k++)
                    {
                        bitCounter += memory[j].Counter[k] ? "1" : "0";

                    }
                    text += "FRAME: " + j.ToString() +
                            "\t ID: " + memory[j].ID.ToString() +
                            "\t REFERENCE BIT: " + memory[j].Reference.ToString() +
                            "\t LOAD TIME: " + memory[j].LoadTime.ToString() +
                            "\t LAST REFERENCE TIME: " + memory[j].LastReferenceTime.ToString() +
                            "\t COUNTER: " + bitCounter +
                            Environment.NewLine;
                }

                if (pagesInMem == 20)
                {
                    for(int k = 20; k < 32; k++)
                    {
                        text += "FRAME: " + k.ToString() +
                                "\t ID: XX" +
                                "\t REFERENCE BIT: XXXX" + 
                                "\t LOAD TIME: XXXX" +
                                "\t LAST REFERENCE TIME: XXXX" +
                                "\t COUNTER: XXXXXXXX" +
                                Environment.NewLine;
                    }
                }
                text += Environment.NewLine +
                        "\t\t CURRENT FAULTS: " + currentFaults.ToString() +
                        "\t\t TOTAL FAULTS: " + totalFaults.ToString() +
                        Environment.NewLine;
                #endregion

                currentTime++;
            }

            richTextBox1.Text = text;
            label1.Text = "TOTAL FAULTS: " + totalFaults.ToString();
        }
    }
}
