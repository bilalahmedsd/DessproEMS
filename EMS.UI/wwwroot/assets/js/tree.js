$(document).ready(function () {
    // Initialize Tree Structure
    $('#tree-container').jstree({
        'core': {
            'data': [
                {
                    "text": "Pakistan", "children": [
                        { "text": "Reports" },
                        { "text": "Data" }
                    ]
                },
                {
                    "text": "Oman", "children": [
                        { "text": "Admins" },
                        { "text": "Relations" }
                    ]
                },
                {
                    "text": "USA", "children": [
                        { "text": "New York" },
                        { "text": "California" }
                    ]
                }
            ]
        },
        "search": { "show_only_matches": true },
        "plugins": ["search"]
    });

    // Toggle Expand/Collapse
    let isExpanded = false;
    $("#toggleTree").click(function () {
        if (isExpanded) {
            $("#tree-container").jstree("close_all");
            $(this).text("Expand All").removeClass("btn-danger").addClass("btn-primary");
        } else {
            $("#tree-container").jstree("open_all");
            $(this).text("Collapse All").removeClass("btn-primary").addClass("btn-danger");
        }
        isExpanded = !isExpanded;
    });

    // Search Functionality
    $("#treeSearch").on("keyup", function () {
        let searchText = $(this).val().trim();
        $("#tree-container").jstree(true).search(searchText);
    });

    // Handle Node Click (Show/Hide Sections)
    $('#tree-container').on("select_node.jstree", function (e, data) {
        let selectedNodeText = data.node.text.toLowerCase().replace(/\s+/g, "-"); // Convert to lowercase & replace spaces
        let fullPath = [];

        // Get full hierarchy path
        let node = data.node;
        while (node.parent !== "#") {
            node = $("#tree-container").jstree().get_node(node.parent);
            fullPath.unshift(node.text.toLowerCase().replace(/\s+/g, "-"));
        }

        fullPath.push(selectedNodeText); // Add the clicked node
        let contentDivId = "#content-" + fullPath.join("-"); // Combine full path as ID

        // Hide all sections and show the selected one
        $(".dynamic-section").hide();
        $("#default-message").hide();

        if ($(contentDivId).length > 0) {
            $(contentDivId).show();
        } else {
            $("#default-message").show();
        }
    });
});
