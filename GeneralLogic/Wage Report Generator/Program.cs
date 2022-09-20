using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

class MainClass
{
    static void Main(string[] args)
    {
        if (args.Length == 1)
        {
            if (args[0] == "-h")
            {
                Console.Write("\nWageReportGenerator.exe Acceptable Arguements\n" +
                               "1: -h Brings up this helpo documentation\n" +
                               "2: jsonInputPath jsonOutputPath\n" +
                               "3: jsonInputPath jsonOutputPath double(standardWeekHours) double(overtimeCap) double(overtimeRate) double(doubletimeRate)\n\n" +

                                "Option 2 Information:\n" +
                                "   -Set the following values by default:\n" +
                                "        -standardWeekHours\n" +
                                "        - overtimeCap: 48\n" +
                                "        - overtimeRate: 1.5\n" +
                                "       - doubletimeRate: 2.0\n"
     );
            }
            else
            {
                Console.Write("\nInvalid Arguements Given: Use Argument -h for proper arguement structures\n");
            }
        }
        else if (args.Length == 2)
        {
            try
            {
                string inputPath = args[0];
                string outputPath = args[1];
                payReportGenerator PayReport = new payReportGenerator(inputPath);
                PayReport.create_WageReport(outputPath);
            }
            catch
            {
                Console.Write("\nInvalid Arguements Given: Use Argument -h for proper arguement structures\n");
            }

        }
        else if (args.Length == 6)
        {
            try
            {
                string inputPath = args[0];
                string outputPath = args[1];
                double standardWeekHours = Convert.ToDouble(args[2]);
                double overtimeCap = Convert.ToDouble(args[3]);
                double overtimeRate = Convert.ToDouble(args[4]);
                double doubletimeRate = Convert.ToDouble(args[5]);

                payReportGenerator PayReport = new payReportGenerator(inputPath, standardWeekHours, overtimeCap, overtimeRate, doubletimeRate);
                PayReport.create_WageReport(outputPath);
            }
            catch
            {
                Console.Write("\nInvalid Arguements Given: Use Argument -h for proper arguement structures\n");
            }
        }
        else
        {
            Console.Write("\nInvalid Arguements Given: Use Argument -h for proper arguement structures\n");
        }
    }
}
class payReportGenerator
{
    public string jsonString = "";
    double regularWorkWeekHours = 40;
    double overtimeCap = 48;
    double overtimeRate = 1.5;
    double doubletimeRate = 2.0;
    public payReportGenerator(string inPathString)
    {
        this.jsonString = read_jsonFile(inPathString);
    }
    public payReportGenerator(string inPathString, double standardWeekHours, double overtimeCap, double overtimeRate, double doubletimeRate)
    {
        this.jsonString = read_jsonFile(inPathString);
        this.regularWorkWeekHours = standardWeekHours;
        this.overtimeCap = overtimeCap;
        this.overtimeRate = overtimeRate;
        this.doubletimeRate = doubletimeRate;
    }
    public async void create_WageReport(string outputPath)
    {
        List<employeeWages> employeeWages;
        string jsonString;

        employeeWages = this.generate_reportObject();
        jsonString = generate_OutputJsonString(employeeWages);
        await File.WriteAllTextAsync(outputPath, jsonString);
    }
    private static string read_jsonFile(string jsonPath)
    {
        string jsonString = File.ReadAllText(jsonPath);
        return jsonString;
    }
    private List<employeeWages> generate_reportObject()
    {

        List<employeeWages> employeeWagesList = new List<employeeWages>();

        jsonInfo inputData = JsonConvert.DeserializeObject<jsonInfo>(this.jsonString);

        foreach (var employeeRecord in inputData.employeeInfo)
        {
            employeeWages wagesRecord = new employeeWages();
            wagesRecord.set_employeeName(employeeRecord.employeeName);
            wagesRecord.set_regularHoursAndTotalWages(employeeRecord.timePunch_list, inputData.jobInfo, regularWorkWeekHours);
            wagesRecord.set_overtimeHoursAndTotalWages(employeeRecord.timePunch_list, inputData.jobInfo, regularWorkWeekHours, overtimeCap, 1.5);
            wagesRecord.set_doubleTimeHoursAndTotalWages(employeeRecord.timePunch_list, inputData.jobInfo, regularWorkWeekHours, overtimeCap, 2);
            wagesRecord.set_doubleVariablesPrecision();
            employeeWagesList.Add(wagesRecord);
        }

        return employeeWagesList;
    }
    private string generate_OutputJsonString(List<employeeWages> employeeWagesReport)
    {
        string outputJsonString = JsonConvert.SerializeObject(employeeWagesReport, Formatting.Indented);
        return outputJsonString;
}
}
public partial class employeeWages
{
    public string employeeName = "";
    public string regular = "0.000";
    public string overTime = "0.000";
    public string doubleTime = "0.000";
    public string wagesTotal = "0.000";
    public string benefitTotal = "0.000";

    public void set_employeeName(string employeName)
    {
        this.employeeName = employeName;
    }
    public void set_regularHoursAndTotalWages(List<timePunch> punchesList, List<jobMeta> jobDataList, double regularWorkWeekHours)
    {
        double hoursAccrued = 0;
        double totalBenefitsWages = 0;
        double totalHourlyWages = 0;
        double totalRegularHours = 0;

        foreach (var punch in punchesList)
        {
            // get punch rate
            double jobBenefitsRate = punch.getBenefitsRate(jobDataList);
            double jobHourlyRate = punch.getBaseRate(jobDataList);
            double punchBillableHours = 0;

            if (hoursAccrued >= regularWorkWeekHours)
            {
                break;
            }
            else if (hoursAccrued + punch.Hours_Worked() <= regularWorkWeekHours)
            {
                hoursAccrued = hoursAccrued + punch.Hours_Worked();
                punchBillableHours = punch.Hours_Worked();

            }
            else if ((hoursAccrued + punch.Hours_Worked() > regularWorkWeekHours))
            {
                double overTimeHoursAccrued = (hoursAccrued + punch.Hours_Worked()) - regularWorkWeekHours;
                hoursAccrued = hoursAccrued + punch.Hours_Worked();
                punchBillableHours = punch.Hours_Worked() - overTimeHoursAccrued;
            }
            double punchBenefitsWages = punchBillableHours * jobBenefitsRate;
            double punchHourlyWages = punchBillableHours * jobHourlyRate;
            totalBenefitsWages = totalBenefitsWages + punchBenefitsWages;
            totalHourlyWages = totalHourlyWages + punchHourlyWages;
            totalRegularHours = totalRegularHours + punchBillableHours;
        }
        this.regular = Convert.ToString(totalRegularHours);
        this.wagesTotal = Convert.ToString(totalHourlyWages);
        this.benefitTotal = Convert.ToString(totalBenefitsWages);
    }
    public void set_overtimeHoursAndTotalWages(List<timePunch> punchesList, List<jobMeta> jobDataList, double regularWorkWeekHours, double overtimeCap, double rateRiser)
    {
        double hoursAccrued = 0;
        double totalBenefitsWages = 0;
        double totalHourlyWages = 0;
        double totalOvertimeHours = 0;

        foreach (var punch in punchesList)
        {
            // get punch rate
            double jobBenefitsRate = punch.getBenefitsRate(jobDataList);
            double jobHourlyRate = punch.getBaseRate(jobDataList) * rateRiser;
            double punchBillableHours = 0;

            if (hoursAccrued >= overtimeCap)
            {
                break;
            }
            else if (hoursAccrued + punch.Hours_Worked() <= overtimeCap && hoursAccrued + punch.Hours_Worked() > regularWorkWeekHours)
            {
                hoursAccrued = hoursAccrued + punch.Hours_Worked();
                punchBillableHours = hoursAccrued - regularWorkWeekHours;

            }
            else if (hoursAccrued + punch.Hours_Worked() > overtimeCap)
            {
                if (hoursAccrued < 40)
                {
                    double punchRegularHoursAccrued = 40 - hoursAccrued;
                    hoursAccrued = hoursAccrued + punch.Hours_Worked();
                    double doubletimeHoursAccrued = hoursAccrued - overtimeCap;
                    punchBillableHours = punch.Hours_Worked() - doubletimeHoursAccrued - punchRegularHoursAccrued;
                }
                else
                {
                    hoursAccrued = hoursAccrued + punch.Hours_Worked();
                    double doubletimeHoursAccrued = hoursAccrued - overtimeCap;
                    punchBillableHours = punch.Hours_Worked() - doubletimeHoursAccrued;
                }
            }
            else
            {
                hoursAccrued = hoursAccrued + punch.Hours_Worked();
            }
            double punchBenefitsWages = punchBillableHours * jobBenefitsRate;
            double punchHourlyWages = punchBillableHours * jobHourlyRate;
            totalBenefitsWages = totalBenefitsWages + punchBenefitsWages;
            totalHourlyWages = totalHourlyWages + punchHourlyWages;

            totalOvertimeHours = totalOvertimeHours + punchBillableHours;
        }
        this.overTime = Convert.ToString(totalOvertimeHours);
        this.wagesTotal = Convert.ToString(Convert.ToDouble(this.wagesTotal) + totalHourlyWages);
        this.benefitTotal = Convert.ToString(Convert.ToDouble(this.benefitTotal) + totalBenefitsWages);
    }

    public void set_doubleTimeHoursAndTotalWages(List<timePunch> punchesList, List<jobMeta> jobDataList, double regularWorkWeekHours, double overtimeCap, double rateRiser)
    {
        double hoursAccrued = 0;
        double totalBenefitsWages = 0;
        double totalHourlyWages = 0;
        double totalDoubletimeHours = 0;

        foreach (var punch in punchesList)
        {
            // get punch rate
            double jobBenefitsRate = punch.getBenefitsRate(jobDataList);
            double jobHourlyRate = punch.getBaseRate(jobDataList) * rateRiser;
            double punchBillableHours = 0;

            if (hoursAccrued + punch.Hours_Worked() > overtimeCap)
            {
                hoursAccrued = hoursAccrued + punch.Hours_Worked();
                punchBillableHours = hoursAccrued - overtimeCap;
            }
            else
            {
                hoursAccrued = hoursAccrued + punch.Hours_Worked();
            }
            double punchBenefitsWages = punchBillableHours * jobBenefitsRate;
            double punchHourlyWages = punchBillableHours * jobHourlyRate;
            totalBenefitsWages = totalBenefitsWages + punchBenefitsWages;
            totalHourlyWages = totalHourlyWages + punchHourlyWages;

            totalDoubletimeHours = totalDoubletimeHours + punchBillableHours;
        }
        this.doubleTime = Convert.ToString(totalDoubletimeHours);
        this.wagesTotal = Convert.ToString(Convert.ToDouble(this.wagesTotal) + totalHourlyWages);
        this.benefitTotal = Convert.ToString(Convert.ToDouble(this.benefitTotal) + totalBenefitsWages);
    }

    public void set_doubleVariablesPrecision()
    {
        this.regular = this.format_DecimalToStringWithXPrecision(Convert.ToDecimal(this.regular), 4);
        this.overTime = this.format_DecimalToStringWithXPrecision(Convert.ToDecimal(this.overTime), 4);
        this.doubleTime = this.format_DecimalToStringWithXPrecision(Convert.ToDecimal(this.doubleTime), 4);
        this.wagesTotal = this.format_DecimalToStringWithXPrecision(Convert.ToDecimal(this.wagesTotal), 4);
        this.benefitTotal = this.format_DecimalToStringWithXPrecision(Convert.ToDecimal(this.benefitTotal), 4);
    }

    public string format_DecimalToStringWithXPrecision(decimal number, int xPrecision)
    {
        string newDecimalString;
        decimal correctDecimal = decimal.Round(number, xPrecision, MidpointRounding.AwayFromZero);
        if (correctDecimal % 1 == 0) 
        {
            newDecimalString = correctDecimal.ToString() + ".000";
        }
        else
        {
            newDecimalString = correctDecimal.ToString();
        }
       

        return newDecimalString;
    }
}
public partial class jsonInfo
    {
        [JsonProperty("jobMeta")]
        public List<jobMeta> jobInfo { get; set; }

        [JsonProperty("employeeData")]
        public List<employeeData> employeeInfo { get; set; }
    }
    public partial class jobMeta
    {
        [JsonProperty("job")]
        public string jobName { get; set; }

        [JsonProperty("rate")]
        public double jobBaseRate { get; set; }

        [JsonProperty("benefitsRate")]
        public double jobBenefitsRate { get; set; }

    }
    public partial class employeeData
    {

        [JsonProperty("employee")]
        public string employeeName { get; set; }

        [JsonProperty("timePunch")]
        public List<timePunch> timePunch_list { get; set; }

    }
    public partial class timePunch
    {
        [JsonProperty("job")]
        public string jobName { get; set; }

        [JsonProperty("start")]
        public DateTime shiftStart { get; set; }

        [JsonProperty("end")]
        public DateTime shiftEnd { get; set; }

        public double Hours_Worked()
        {
            double calcedHours;
            int secondsInHour = 3600;

            TimeSpan shift = this.shiftEnd - this.shiftStart;
            calcedHours = shift.TotalSeconds / secondsInHour;

            return calcedHours;
        }

        public double getBenefitsRate(List<jobMeta> jobDataList)
        {
            foreach (var jobMeta in jobDataList)
            {
                if (jobMeta.jobName == this.jobName)
                {
                    return jobMeta.jobBenefitsRate;
                }
            }
            return 0;
        }

        public double getBaseRate(List<jobMeta> jobDataList)
        {
            foreach (var jobMeta in jobDataList)
            {
                if (jobMeta.jobName == this.jobName)
                {
                    return jobMeta.jobBaseRate;
                }
            }
            return 0;
        }
    }




