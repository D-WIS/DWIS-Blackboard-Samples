The steps to run the demo:

1. Start ddhub server, first try local server:
   - Local server, no existing variables: `docker run  -dit --name blackboard-local -P -p 48030:48030/tcp --hostname localhost  digiwells/ddhubserver:latest`
   - Online server, try with caution, do not pollute the database: `docker run  -dit --name blackboard -P -p 48030:48030/tcp --hostname localhost  digiwells/ddhubserver:latest --useHub --hubURL https://dwis.digiwells.no/blackboard/applications`
2. Install the dependencies: `pip install -r Samples\python_demo\requirements.txt`
3. Run the demo: `python demo.py`

An output example:

```
Example 1: Resolve query
{'VariablesHeader': ['?SIGNAL'], 'Results': [], 'Count': 0, 'IsReadOnly': False}

Example 2: Register query
Result JSON: {'QueryID': 'QueryClient1423953044',
'QueryResultID': 'ns=2;s=QueryDiff1423953044',
'Added': [], 'Removed': []}
New node ID: ns=2;s=QueryClient1423953044

Example 3: Inject manifest
Result JSON: {'Success': False, 'InjectedVariableNamespace': None,
'InjectedNodesNamespace': None, 'ProvidedVariablesNamespace': None,
'InjectedVariables': None,
'InjectedNodes': None, 'ProvidedVariables': None}
```
