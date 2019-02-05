﻿const uri = "api/todo";
let cnt = 0;
$(document).ready(function () {
    //getData();
    //alert("document ready");
});
//function getfrom() { let hel = $("#itemid"); let id = hel.value; get(id); }
function getfrom() { let hel = document.getElementById("itemid"); let id = hel.value; get(id); }
function get(id) {
    $.ajax({
        type: "GET",
        accepts: "application/json",
        //url: "data/get?id=" + id + "&t=" + encodeURIComponent(new Date()),
        contentType: "application/json",
        //contentType: "text/xml",
        //data: JSON.stringify(item),
        url: "data/get",
        data: {id: id, dt: Date()},
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Something went wrong!");
        },
        success: function (result) {
            let str = converttoplainstring(result);
            const vvrr = $("#viewresult");
            vvrr.empty();
            vvrr.append($("<span>" + str + "</span>"));
        }
    });
}
function converttoplainstring(record) {
    let str = "<a href='javascript:void(0)' onclick='get(\"" + record.id + "\")'>@</a>" +
        " <span style='color: green;'>" + record.ty + "</span>";
    $.each(record.arcs,
        function (i, item) {
            let alt = item.alt;
            str += " <span style='color: blue;'>" + item.prop + "</span>";
            if (alt === "field") {
                str += " " + item.text;
            } else if (alt === "direct") {
                str += " " + converttoplainstring(item.rec);
            } else if (alt === "inverse") {
                $.each(item.recs,
                    function (j, rec) {
                        str += " " + converttoplainstring(rec);
                    });
            }
        });
    return str;
}





// ================================================ исходный вариант =====================
let todos = null;
function getCount(data) {
    const el = $("#counter");
    let name = "to-do";
    if (data) {
        if (data > 1) {
            name = "to-dos";
        }
        el.text(data + " " + name);
    } else {
        el.text("No " + name);
    }
}

//$(document).ready(function () {
//    getData();
//});

function getData() {
    $.ajax({
        type: "GET",
        url: uri,
        cache: false,
        success: function (data) {
            const tBody = $("#todos");

            $(tBody).empty();

            getCount(data.length);

            $.each(data, function (key, item) {
                const tr = $("<tr></tr>")
                    .append(
                        $("<td></td>").append(
                            $("<input/>", {
                                type: "checkbox",
                                disabled: true,
                                checked: item.isComplete
                            })
                        )
                    )
                    .append($("<td></td>").text(item.name))
                    .append(
                        $("<td></td>").append(
                            $("<button>Edit</button>").on("click", function () {
                                editItem(item.id);
                            })
                        )
                    )
                    .append(
                        $("<td></td>").append(
                            $("<button>Delete</button>").on("click", function () {
                                deleteItem(item.id);
                            })
                        )
                    );

                tr.appendTo(tBody);
            });

            todos = data;
        }
    });
}

function addItem() {
    const item = {
        name: $("#add-name").val(),
        isComplete: false
    };

    $.ajax({
        type: "POST",
        accepts: "application/json",
        url: uri,
        contentType: "application/json",
        data: JSON.stringify(item),
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Something went wrong!");
        },
        success: function (result) {
            getData();
            $("#add-name").val("");
        }
    });
}

function deleteItem(id) {
    $.ajax({
        url: uri + "/" + id,
        type: "DELETE",
        success: function (result) {
            getData();
        }
    });
}

function editItem(id) {
    $.each(todos, function (key, item) {
        if (item.id === id) {
            $("#edit-name").val(item.name);
            $("#edit-id").val(item.id);
            $("#edit-isComplete")[0].checked = item.isComplete;
        }
    });
    $("#spoiler").css({ display: "block" });
}

$(".my-form").on("submit", function () {
    const item = {
        name: $("#edit-name").val(),
        isComplete: $("#edit-isComplete").is(":checked"),
        id: $("#edit-id").val()
    };

    $.ajax({
        url: uri + "/" + $("#edit-id").val(),
        type: "PUT",
        accepts: "application/json",
        contentType: "application/json",
        data: JSON.stringify(item),
        success: function (result) {
            getData();
        }
    });

    closeInput();
    return false;
});

function closeInput() {
    $("#spoiler").css({ display: "none" });
}