# Azure Digital Twins Model Uploader
This project attempts to upload your entire ADT ontology in the least amount of API calls. 

## Usage
There are 3 parameters
- -p The path to your DTDL models. All sub-directories will also be searched for `.json` files.
- -u The url of you ADT instance. This includes `https://`.
- -c The ID of an application registration in Azure AD that has Data Writer rights on Azure Digital Twins.

## Notes
There is still an issue where a large batch of models will trigger a `400` error in ADT, even though all the model's dependencies are met. This is a known issue and I'm working to resolve it.