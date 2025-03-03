using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace RevitAddinBootcamp
{
    public class AppSkills : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            //1. create tab
            string tabName = "MyTab";
            //application.CreateRibbonTab(tabName);


            //1b safer creation
            try
            {   
                application.CreateRibbonTab(tabName);
            }
            catch (Exception error)
            {
                Debug.Print("Tab already Exists Use Existing Panel");
                Debug.Print(error.Message);
            }

            //2 create panel
            string panelName1 = "Test Panel";
            string panelName2 = "Test Panel 2";
            //string panelName3 = "Test Panel 3";


            RibbonPanel panel = application.CreateRibbonPanel(tabName, panelName1);
            RibbonPanel panel2 = application.CreateRibbonPanel(panelName2);
            //RibbonPanel panel3 = app.CreateRibbonPanel("Archictecture", panelName3);

            //2a get existing panel
            List<RibbonPanel> panelList = application.GetRibbonPanels();
            List<RibbonPanel> panelList2 = application.GetRibbonPanels(tabName);

            //2b create/get panel safe method
            RibbonPanel panel4 = CreateGetPanel(application, tabName, panelName1);

            //3 create button data
            PushButtonData buttonData1 = new PushButtonData("button1", "cmdChallenge04", Assembly.GetExecutingAssembly().Location, "RevitAddinBootcamp.cmdChallenge04");

            PushButtonData buttonData2 = new PushButtonData("button2", "cmdChallenge03", Assembly.GetExecutingAssembly().Location, "RevitAddinBootcamp.cmdChallenge03");

            //4 tooltips
            buttonData1.ToolTip = "this is command 1";
            buttonData2.ToolTip = "this is command 2";

            //5 add image
            buttonData1.LargeImage = ConvertToImageSource(Properties.Resources.TestImage);
            buttonData1.Image = ConvertToImageSource(Properties.Resources.TestImagesmall);
            buttonData2.LargeImage = ConvertToImageSource(Properties.Resources.Module01);
            buttonData2.Image = ConvertToImageSource(Properties.Resources.Module01small);

            //6 create buttons
            //PushButton button1 = panel.AddItem(buttonData1) as PushButton;
            //PushButton button2 = panel.AddItem(buttonData2) as PushButton;

            //7 Create Stacked Buttons
            //panel.AddStackedItems(buttonData1, buttonData2);

            //8 Split button
            //SplitButtonData splitButtonData = new SplitButtonData("splitButton", "Split\rButton");
            //SplitButton splitButton = panel.AddItem(splitButtonData) as SplitButton;
            //splitButton.AddPushButton(buttonData1);
            //splitButton.AddPushButton(buttonData2);

            //10 other items
            //this is the dropdown at the bottom of the panel
            panel.AddSlideOut();

            panel.AddSeparator();


            //9 add pulldown button
            PulldownButtonData pulldownButtonData = new PulldownButtonData("pullDownButton", "Pulldown\rButton");
            pulldownButtonData.LargeImage = ConvertToImageSource(Properties.Resources.TestImage);

            PulldownButton pullDownButton = panel.AddItem(pulldownButtonData) as PulldownButton;
            pullDownButton.AddPushButton(buttonData1);
            pullDownButton.AddPushButton(buttonData2);




            return Result.Succeeded;

        }

        private RibbonPanel CreateGetPanel(UIControlledApplication app, string tabName, string panelName1)
        {
            foreach(RibbonPanel panel in app.GetRibbonPanels(tabName))
            {
                if (panel.Name == panelName1)
                {
                    return panel;
                }
            }
            //RibbonPanel returnPanel = app.CreateRibbonPanel(tabName, panelName1);
            //return returnPanel;

            return app.CreateRibbonPanel(tabName, panelName1);
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }

        public BitmapImage ConvertToImageSource(byte[] imageData)
        {
            using (MemoryStream mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                BitmapImage bmi = new BitmapImage();
                bmi.BeginInit();
                bmi.StreamSource = mem;
                bmi.CacheOption = BitmapCacheOption.OnLoad;
                bmi.EndInit();

                return bmi;
            }
        }

    }
}
