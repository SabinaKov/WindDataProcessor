# WindDataProcessor
For ZKL - Výzkum a Vývoj, a.s. company; Processing technical bearing loading data by customer
1. to the state, which is maintainable with ZKL software tools.
2. for calculation of bearing force reactions.


Console application.

Ad 1. - Processing technical bearing loading data by customer to the state, which is maintainable with ZKL software tools.
Using histogram processing reduces data from LTS state to LDD.
Method:
DataProcessor dataProcessor = new DataProcessor(...);
dataProcessor.Process();

Ad 2. - Processing technical bearing loading data by customer for calculation of bearing force reactions.
New method was involved. This method uses linear interpolation of stiffness points calculated with different software (e.g. Mesys) for axial reaction calculation
Method:
DataProcessor dataProcessor = new DataProcessor(...);
await dataProcessor.BearingReactions();

Start with Program.cs file for specific customer (Gamesa, Nanjing, ...)
