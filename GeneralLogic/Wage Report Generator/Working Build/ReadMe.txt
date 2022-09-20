Name: WageReportGenerator.exe

Purpose: To parse a Standard Wage json from eBacon into a WageReport.Json

Use: 
	1: -h Brings up this helpo documentation
	2: jsonInputPath jsonOutputPath
	3: jsonInputPath jsonOutputPath double(standardWeekHours) double(overtimeCap) double(overtimeRate) double(doubletimeRate)

	Option 2 Information:
   	-Set the following values by default:
        	-standardWeekHours
        	- overtimeCap: 48
        	- overtimeRate: 1.5
       	- doubletimeRate: 2.0

Test Strategy:
	Validation 1:
		Purpose: Test that the output is as expected and matches Ebacon's results from thier input given.
		Input File: Wage Report Generator/Working Build/Testing Inputs and Outputs/TestOriginal.json
		Output File: Wage Report Generator/Working Build/Testing Inputs and Outputs/OutputOriginal.json
		Iterations:
			Test 1: Failed double time wages calculation incorrect
			Solution: payReportGenerator default value for doubletimeRate was accidently set to 1.5 needed changed to 2
			Test 2: Test 1 issue Resolved. Output As Doubles not Strings/Also incorrect format with integers
			Solution: Changed class employeeWages attributes so they are all string types, implemented format_DecimalToStringWithXPrecision,
			          and made all set_functions generating data call it for consitent output.
			Test 3: Test 2 issue Resolved
	
	Validation 2:
		Purpose: Stress testing this program with some edge cases. :-)
		Input File: Wage Report Generator/Working Build/Testing Inputs and Outputs/TestValidation2.json
		Output File: Wage Report Generator/Working Build/Testing Inputs and Outputs/OutputValidation2.json
		Cases in Json:
		      Dick - Case of the miniscule work time. Works 1 Job for 1 minute ensure small numbers work well. 
		      Jane - Case of large worktime/shifts. Ensures that large shifts will be adequately handled by the program.
		Iterations:
			Test 1: Case - Dick Passed, Case Jane Overtime hours calculated incorrectly and are exceeding 8 hours. Secondary issue wage calcs off.
			Solution : Added if logic in payReportGenerator's set_overtime... function so that massive shifts starting in regular hours but
			ending in doubletime hours where calculated correctly.
			Test 2: Test 1 Issues resolved.
			
	Validation 3 - NOT IMPLEMENTED: 
		Purpose: Ressiliancy/Error testing this program with more cases.
		Input File: Wage Report Generator/Working Build/Testing Inputs and Outputs/TestValidation3.json
		Output File: Wage Report Generator/Working Build/Testing Inputs and Outputs/OutputValidation3.json
		Cases in Json:
		      Harry - Case of negative time stamps.  
		      Frank - Case empty employee json.
			Jim - Case of invalid date input.


