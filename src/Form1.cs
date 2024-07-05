using BizHawk.Client.Common;
using BizHawk.Client.EmuHawk;
using BizHawk.Emulation.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    [ExternalTool("transition-tool")]
    public partial class Form1 : ToolFormBase, IExternalToolForm
    {
        public ApiContainer? _maybeAPIContainer { get; set; }
        private ApiContainer APIs => _maybeAPIContainer!;
        public Form1() => InitializeComponent();


        protected override string WindowTitleStatic => "transition-tool";
        

        public override void Restart()
        {
            //ran when external tool is started/restarted
            APIs.Memory.UseMemoryDomain(APIs.Memory.MainMemoryName);
            
        }


        private long sizeAddressPtr = 0x00140cc0;
        private long tileListAddressPtr = 0x00140cd0;
        
        protected override void UpdateAfter()
        {
            //this is done so that it scales properly
            int tileSize = APIs.EmuClient.TransformPoint(new Point(8, 8)).X;
            
            
            uint areaSizeAddr = APIs.Memory.ReadU32(sizeAddressPtr)-0x02000000; // -0x02000000 is necessary for bizhawk
            int areaWidth = APIs.Memory.ReadS16(areaSizeAddr);
            
            int areaHeight = APIs.Memory.ReadS16(areaSizeAddr + 2);
            IReadOnlyList<Byte> tileList = APIs.Memory.ReadByteRange(APIs.Memory.ReadU32(tileListAddressPtr) - 0x02000000, areaHeight*areaWidth);
            


            //get cam pos
            System.Numerics.Vector2 CamPos = new(APIs.Memory.ReadU32(0x00140f4c), APIs.Memory.ReadU32(0x00140f50));
            System.Numerics.Vector2 CamTilePos = new((CamPos.X + 300) / 2048, (CamPos.Y + 300) / 2048);
            if (CamPos.X < 0) CamPos.X = 0;
            if (CamPos.Y < 0) CamPos.Y = 0;
            
            APIs.Gui.WithSurface(DisplaySurfaceID.Client, () =>
            {
                for (int x = 0; x < 32; x++)
                {
                    for (int y = 0; y < 24; y++)
                    {
                        if (((int)CamTilePos.Y + y) * areaWidth + (int)CamTilePos.X + x < tileList.Count) {
                            int tile = tileList[((int)CamTilePos.Y + y) * areaWidth + (int)CamTilePos.X + x];
                            

                            Point relPos = new(x*tileSize,  y*tileSize);
                            Point scrPoint = new Point(relPos.X, relPos.Y + APIs.EmuClient.ScreenHeight()/2);
                            
                            Color color = Color.FromArgb((int)(0xEE500000 + tile*10+tile*10*256));
                            if (tile != 0)
                                APIs.Gui.DrawRectangle(scrPoint.X, scrPoint.Y, tileSize, tileSize, Color.Red, color);
                            
                        }
                        
                    }
                }
            });
        }


    }
}
