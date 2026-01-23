import json
import os
from opcua import Client, ua

# URL of the OPC UA server to connect to
server_url = "opc.tcp://localhost:48030/"
current_directory = os.path.dirname(os.path.abspath(__file__))


def read_value(client, node_id):
    """
    Reads the value of the specified node.

    Parameters:
        node_id (str): The node ID of the data to be read.
    """
    node = client.get_node(node_id)
    value = node.get_value()
    print(f"Value of node {node_id}: {value}")


def browse_children_nodes(client, object_node_id):
    """
    Browses and prints the children of the specified object node.

    Parameters:
        client: The OPC UA client instance.
        object_node_id (str): The node ID of the object whose children to browse.
    """
    node = client.get_node(object_node_id)
    print(f"Browsing children of node: {node}")
    children = node.get_children()
    for child in children:
        print(f"Child: {child} | Browse Name: {child.get_browse_name()} | Node ID: {child.nodeid}")


def call_method_explicit_parent(client, object_node_id, method_node_id, input_args):
    """
    Calls a method on the specified object node with given input arguments.
    Requires explicit parent object node ID.

    Parameters:
        client: The OPC UA client instance.
        object_node_id (str): The node ID of the parent object that has the method.
        method_node_id (str): The node ID of the method to be called.
        input_args (list): The list of input arguments for the method.
    """
    object_node = client.get_node(object_node_id)
    method_node = client.get_node(method_node_id)
    # input_args = [ua.Variant(25, ua.VariantType.Double), ua.Variant(26, ua.VariantType.Double)]
    result = object_node.call_method(method_node, *input_args)
    # print(f"Result of method call: {result}")
    return result


def call_method_auto_parent(client, method_node_id, input_args):
    """
    Calls a method on the specified node with given input arguments.
    Automatically finds the parent object node from the method node.

    Parameters:
        client: The OPC UA client instance.
        method_node_id (str): The node ID of the method to be called.
        input_args (list): The list of input arguments for the method.
    """
    method = client.get_node(method_node_id)
    # Find parent object by inverse references (method belongs to object)
    parents = method.get_references(direction=ua.BrowseDirection.Inverse)

    if not parents:
        raise RuntimeError("Failed to find the parent object of given method node. Please provide the parent object node ID directly.")

    parent_node_id = parents[0].NodeId
    parent = client.get_node(parent_node_id)
    result = parent.call_method(method, *input_args)

    # Alternative implementation (commented out):
    # method_node = client.get_node(method_node_id)
    # result = method_node.call_method(method_node, *input_args)

    return result


def run():
    """
    Main function that manages the connection to the OPC UA server and executes
    the read and method call operations.
    """
    try:
        # Connect to the server
        # Create a client instance
        client = Client(server_url)
        client.connect()
        print(f"Connected to {server_url}")

        # Load the SPARQL query string from file
        with open(current_directory + "/query_string.ttl", "r") as file:
            query_string = file.read()

        # Example 1: Resolve query - sends a SPARQL query and retrieves the response
        print("\nExample 1: Resolve query")
        input_args = [ua.Variant(value=[query_string], varianttype=ua.VariantType.String, is_array=True)]
        result_jsonstr = call_method_auto_parent(client, "ns=2;s=ResolveQuery", input_args)
        result_json = json.loads(result_jsonstr)
        print(result_json)

        # Example 2: Register query - sends a SPARQL query and gets the node ID of a node to monitor
        print("\nExample 2: Register query")
        input_args = [ua.Variant(value=[query_string], varianttype=ua.VariantType.String, is_array=True)]
        result_jsonstr, new_node_id = call_method_auto_parent(client, "ns=2;s=RegisterQuery", input_args)
        result_json = json.loads(result_jsonstr)
        print("Result JSON:", result_json)
        print("New node ID:", new_node_id)

        # Example 3: Inject manifest - loads and injects manifest data
        print("\nExample 3: Inject manifest")
        with open(current_directory + "/manifest_demo.json", "r") as file:
            query_string = file.read()

        input_args = [ua.Variant(value=[query_string], varianttype=ua.VariantType.String, is_array=True)]
        result_jsonstr = call_method_auto_parent(client, "ns=2;s=Inject", input_args)
        result_json = json.loads(result_jsonstr)
        print("Result JSON:", result_json)

    except Exception as e:
        # Handle any exceptions that occur
        print(f"An error occurred: {e}")

    finally:
        # Ensure the client disconnects from the server
        client.disconnect()
        print(f"Disconnected from {server_url}")


if __name__ == "__main__":
    run()
