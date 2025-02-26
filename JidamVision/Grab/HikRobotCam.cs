using MvCamCtrl.NET;
using OpenCvSharp.Dnn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JidamVision.Grab
{
    internal class HikRobotCam
    {

        private MyCamera _camera = null;

        internal bool Create()
        {

            int nRet = MyCamera.MV_OK;
            _camera = new MyCamera();
            IntPtr pBufForConvert = IntPtr.Zero;

            MyCamera.MV_CC_DEVICE_INFO_LIST stDevList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
            nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref stDevList);
            if (MyCamera.MV_OK != nRet)
            {
                Console.WriteLine("Enum device failed:{0:x8}", nRet);
                return false;
            }
            Console.WriteLine("Enum device count :{0} \n", stDevList.nDeviceNum);
            if (0 == stDevList.nDeviceNum)
            {
                return false;
            }

            MyCamera.MV_CC_DEVICE_INFO stDevInfo;

            // ch:打印设备信息 | en:Print device info
            for (Int32 i = 0; i < stDevList.nDeviceNum; i++)
            {
                stDevInfo = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(stDevList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));

                if (MyCamera.MV_GIGE_DEVICE == stDevInfo.nTLayerType)
                {
                    MyCamera.MV_GIGE_DEVICE_INFO stGigEDeviceInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(stDevInfo.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                    uint nIp1 = ((stGigEDeviceInfo.nCurrentIp & 0xff000000) >> 24);
                    uint nIp2 = ((stGigEDeviceInfo.nCurrentIp & 0x00ff0000) >> 16);
                    uint nIp3 = ((stGigEDeviceInfo.nCurrentIp & 0x0000ff00) >> 8);
                    uint nIp4 = (stGigEDeviceInfo.nCurrentIp & 0x000000ff);
                    Console.WriteLine("[device " + i.ToString() + "]:");
                    Console.WriteLine("DevIP:" + nIp1 + "." + nIp2 + "." + nIp3 + "." + nIp4);
                    Console.WriteLine("UserDefineName:" + stGigEDeviceInfo.chUserDefinedName + "\n");
                }
                else if (MyCamera.MV_USB_DEVICE == stDevInfo.nTLayerType)
                {
                    MyCamera.MV_USB3_DEVICE_INFO stUsb3DeviceInfo = (MyCamera.MV_USB3_DEVICE_INFO)MyCamera.ByteToStruct(stDevInfo.SpecialInfo.stUsb3VInfo, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                    Console.WriteLine("[device " + i.ToString() + "]:");
                    Console.WriteLine("SerialNumber:" + stUsb3DeviceInfo.chSerialNumber);
                    Console.WriteLine("UserDefineName:" + stUsb3DeviceInfo.chUserDefinedName + "\n");
                }
            }

            Int32 nDevIndex = 0;
            Console.Write("Please input index(0-{0:d}):", stDevList.nDeviceNum - 1);
            try
            {
                nDevIndex = Convert.ToInt32(Console.ReadLine());
            }
            catch
            {
                Console.Write("Invalid Input!\n");
                return false;
            }

            if (nDevIndex > stDevList.nDeviceNum - 1 || nDevIndex < 0)
            {
                Console.Write("Input Error!\n");
                return false;
            }
            stDevInfo = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(stDevList.pDeviceInfo[nDevIndex], typeof(MyCamera.MV_CC_DEVICE_INFO));

            // ch:创建设备 | en: Create device
            nRet = _camera.MV_CC_CreateDevice_NET(ref stDevInfo);
            if (MyCamera.MV_OK != nRet)
            {
                Console.WriteLine("Create device failed:{0:x8}", nRet);
                return false;
            }


            return true;
        }





        internal bool Open()
        {
            int nRet = MyCamera.MV_OK;
            nRet = _camera.MV_CC_OpenDevice_NET();

            if (MyCamera.MV_OK != nRet)
            {
                Console.WriteLine("Open device failed:{0:x8}", nRet);
                return false;
            }

            int nPacketSize = _camera.MV_CC_GetOptimalPacketSize_NET();
            if (nPacketSize > 0)
            {
                nRet = _camera.MV_CC_SetIntValue_NET("GevSCPSPacketSize", (uint)nPacketSize);
                if (nRet != MyCamera.MV_OK)
                {
                    Console.WriteLine("Warning: Set Packet Size failed {0:x8}", nRet);
                }
            }
            else
            {
                Console.WriteLine("Warning: Get Packet Size failed {0:x8}", nPacketSize);
            }


            // ch:设置触发模式为off || en:set trigger mode as off
            nRet = _camera.MV_CC_SetEnumValue_NET("TriggerMode", 0);
            if (MyCamera.MV_OK != nRet)
            {
                Console.WriteLine("Set TriggerMode failed:{0:x8}", nRet);
                return false;
            }

            // ch:开启抓图 || en: start grab image
            nRet = _camera.MV_CC_StartGrabbing_NET();
            if (MyCamera.MV_OK != nRet)
            {
                Console.WriteLine("Start grabbing failed:{0:x8}", nRet);
                return false;
            }
            nRet = _camera.MV_CC_OpenDevice_NET();
            if (MyCamera.MV_OK != nRet)
            {
                Console.WriteLine("Open device failed:{0:x8}", nRet);
                return false;
            }

            if (nPacketSize > 0)
            {
                nRet = _camera.MV_CC_SetIntValue_NET("GevSCPSPacketSize", (uint)nPacketSize);
                if (nRet != MyCamera.MV_OK)
                {
                    Console.WriteLine("Warning: Set Packet Size failed {0:x8}", nRet);
                }
            }
            else
            {
                Console.WriteLine("Warning: Get Packet Size failed {0:x8}", nPacketSize);
            }

            // ch:设置触发模式为off || en:set trigger mode as off
            nRet = _camera.MV_CC_SetEnumValue_NET("TriggerMode", (int)MyCamera .MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON );
            if (MyCamera.MV_OK != nRet)
            {
                Console.WriteLine("Set TriggerMode failed:{0:x8}", nRet);
                return false;
            }

            // ch:开启抓图 || en: start grab image
            nRet = _camera.MV_CC_StartGrabbing_NET();
            if (MyCamera.MV_OK != nRet)
            {
                Console.WriteLine("Start grabbing failed:{0:x8}", nRet);
                return false;
            }

            return true;
        }








        internal bool Grab()
        {
            int nRet = _camera.MV_CC_SetCommandValue_NET("TriggerSoftware");
            if (MyCamera.MV_OK != nRet)
            {
                Console.WriteLine("Trigger Software Fail!:{0:x8}", nRet);
                return false;
            }

            return true;
        }



        internal bool Close()
        {

            int nRet = MyCamera.MV_OK;

            nRet = _camera.MV_CC_StopGrabbing_NET();
            if (MyCamera.MV_OK != nRet)
            {
                Console.WriteLine("Stop grabbing failed:{0:x8}", nRet);
                return false;
            }

            // ch:关闭设备 | en:Close device
            nRet = _camera.MV_CC_CloseDevice_NET();
            if (MyCamera.MV_OK != nRet)
            {
                Console.WriteLine("Close device failed:{0:x8}", nRet);
                return false;
            }

            // ch:销毁设备 | en:Destroy device
            nRet = _camera.MV_CC_DestroyDevice_NET();
            if (MyCamera.MV_OK != nRet)
            {
                Console.WriteLine("Destroy device failed:{0:x8}", nRet);
                return false;
            }


            return true;
        }
    }
}
