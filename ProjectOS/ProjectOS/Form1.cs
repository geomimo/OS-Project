using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
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

        //Locate form and richTextBox1.
        private void Form1_Load(object sender, EventArgs e)
        {
            this.Location = new Point(0, 0);
            this.Height = Screen.PrimaryScreen.Bounds.Height - 50;
            richTextBox1.Size = new Size(this.Width - 100, this.Height - 100);
        }

        //Generate the sequence.
        private void GenerateSeq()
        {  
            Random r = new Random();
            text += Environment.NewLine + "SEQUENCE: ";

            // From memory/disk take all current pages, select their IDs, suffle them (OrderBy Guid) and return that list. 
            List<int> randMemList = memory.Take(pagesInMem).Select(x => x.ID).OrderBy(a => Guid.NewGuid()).ToList();
            List<int> randDiskList = disk.Select(x => x.ID).OrderBy(a => Guid.NewGuid()).ToList();

            for(int i = 0; i < 20; i++)
            {
                //select a page from memory with 60% probability
                if (r.Next(100) < 60 && pagesInMem != 0)
                {
                    sequence[i] = randMemList.First();
                    randMemList.RemoveAt(0);
                }
                //select a page from disk with 40% probability
                else
                {
                    while (sequence.Contains(randDiskList.First()))
                    {
                        randDiskList.RemoveAt(0);
                    }
                    sequence[i] = randDiskList.First();
                    randDiskList.RemoveAt(0);
                }
            }

            for (int i = 0; i < 20; i++)
            {
                text += sequence[i].ToString() + " ";
            }
            text += Environment.NewLine;
        }

        //Find the Page from memory with the minimum counter.
        private Page FindMin()
        {
            int min = Int32.MaxValue;
            Page minPage = null;
            for(int i = 0; i < pagesInMem; i++)
            {
                //Calculate the counter.
                int counter = 0;
                for (int j = 0; j < 8; j++)
                {
                    counter += memory[i].Counter[j] ? (int)Math.Pow(2D, (double)(7 - j)) : 0;
                }
                MessageBox.Show(counter.ToString());
                //Find minimum.
                if (min > counter)
                {
                    min = counter;
                    minPage = memory[i];
                }
            }
            return minPage;
        }

        //Initialising variables and start simulation.
        private void StartBtn_Click(object sender, EventArgs e)
        {
            StartBtn.Enabled = false;
            totalFaults = 0;
            currentTime = 0;
            pagesInMem = 0;
            progressBar1.Value = 0;
            richTextBox1.Text = String.Empty;
            text = String.Empty;
            label1.Text = "TOTAL FAULTS: ";

            disk = new Page[128];
            memory = new Page[32];
            sequence = new int[20];
            Page.Fill(disk);
            Random r = new Random();

            //Just a pause, before starting.
            Thread.Sleep(250);

            for (int i = 0; i < 500; i++)
            {          
                progressBar1.PerformStep();
                text += "INTERRUPT: " + (i + 1).ToString();
                GenerateSeq();
                currentFaults = 0;
                //Shifting all counters.
                for(int j = 0; j < pagesInMem; j++)
                {
                    memory[j].Reference = false;
                    memory[j].ShiftCounter();
                }

                for (int j = 0; j < 20; j++)
                {
                    
                    Page callingPage = disk[sequence[j]].Copy();
                    //The callingPage exists in memory.
                    if (callingPage.Position != -1)
                    {
                        //Update info.
                        memory[callingPage.Position].Reference = true;
                        memory[callingPage.Position].ShiftCounter();
                        memory[callingPage.Position].Counter[0] = true;
                        memory[callingPage.Position].LastReferenceTime = currentTime;
                    }
                    //The callingPage doesn't exist in memory.
                    else
                    {
                        int indexMem;
                        //Memory isn't full.
                        if (pagesInMem < 32)
                        {
                            indexMem = pagesInMem++;
                        }
                        //Memory is full.
                        else
                        {
                            //Find minimum counter, replace and update info on disk.
                            indexMem = FindMin().Position;
                            int indexDisk = memory[indexMem].ID;
                            disk[indexDisk] = memory[indexMem].Copy();
                            disk[indexDisk].Reference = false;
                            disk[indexDisk].Counter.SetAll(false);
                            disk[indexDisk].Position = -1;
                        }
                        //Update in on memory.
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

                //Display info.
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
                            "\t\t REFERENCE BIT: " + memory[j].Reference.ToString() +
                            "\t\t LOAD TIME: " + memory[j].LoadTime.ToString() +
                            "\t\t LAST REFERENCE TIME: " + memory[j].LastReferenceTime.ToString() +
                            "\t\t COUNTER: " + bitCounter +
                            Environment.NewLine;
                }

                if (pagesInMem < 32)
                {
                    for(int k = pagesInMem; k < 32; k++)
                    {
                        text += "FRAME: " + k.ToString() +
                                "\t ID: XX" +
                                "\t\t REFERENCE BIT: XXXX" +
                                "\t\t LOAD TIME: XXXX" +
                                "\t\t LAST REFERENCE TIME: XXXX" +
                                "\t\t COUNTER: XXXXXXXX" +
                                Environment.NewLine;
                    }
                }
                text += Environment.NewLine +
                        "\t\t CURRENT FAULTS: " + currentFaults.ToString() +
                        "\t\t TOTAL FAULTS: " + totalFaults.ToString() +
                        Environment.NewLine +
                        Environment.NewLine;

                //Clear text every 50 interrupts.
                if (i % 50 == 0)
                {
                    richTextBox1.Text += text;
                    text = String.Empty;
                }
                #endregion

                currentTime++;
            }
            richTextBox1.Text += text;
            StartBtn.Enabled = true;
            label1.Text = "TOTAL FAULTS: " + totalFaults.ToString();
        }

    }
}
