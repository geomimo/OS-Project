using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjectOS
{
    class Page
    {
        public bool Reference { get; set; }
        public int LoadTime { get; set; }
        public int LastReferenceTime { get; set; }
        public int ID { get; set; }
        public BitArray Counter { get; set; }   //represents the M bit counter.
        public int Position { get; set; }    //-1 if it doesn't exist in memory.

        //initialization
        public Page(int ID)
        {
            this.Reference = true;
            this.LoadTime = 0;
            this.LastReferenceTime = 0;
            this.ID = ID;
            this.Counter = new BitArray(8, false);
            this.Position = -1;
        }

        //makes a copy of the Page instance and returns it
        public Page Copy()
        {
            Page newPage = new Page(-1);
            newPage.Reference = this.Reference;
            newPage.LoadTime = this.LoadTime;
            newPage.LastReferenceTime = this.LastReferenceTime;
            newPage.ID = this.ID;
            newPage.Counter = this.Counter;
            newPage.Position = this.Position;
            return newPage;
        }

        //fills the disk with new Pages.
        public static void Fill(Page[] disk)
        {
            for(int i = 0; i < disk.Length; i++)
            {
                disk[i] = new Page(i);
            }
        }

        //shifts right the bits of the counter.
        public void ShiftCounter()
        {
            for (int i = 0; i < 7; i++)
            {
                Counter[7-i] = Counter[6-i];
            }

            Counter[0] = false;
        }
    }
}
