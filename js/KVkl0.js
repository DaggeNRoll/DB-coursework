Survey.StylesManager.applyTheme("default");

var surveyValueChanged = function (sender, options) {
    var el = document.getElementById(options.name);
    if (el) {
        el.value = options.value;
    }
};

var json1 = {
    "title": "Критерии включения",
    "description": "(Для включения пациента в исследование все ответы должны быть \"Да\")",
    triggers: [
        {
            type: "complete",
            expression: "{exit1} = 'Yes'"
        }, {
            type: "complete",
            expression: "{exit2} = 'Yes'"
        }
    ],

    "elements": [

        {
            "isRequired": true,
            "type": "boolean",
            "name": "AgeBetween40_65",
            "title": "Возраст 40 – 65 лет:",
            "labelTrue": "Да",
            "labelFalse": "Нет",
        },
        {
            "isRequired": true,
            "type": "boolean",
            "name": "LowAndModerateRiskOfCardiovascularComplications",
            "title": "Низкий и умеренный риск сердечно – сосудистых осложнений (0-\n" +
                "4% по шкале SCORE):",
            "labelTrue": "Да",
            "labelFalse": "Нет",
        },
        {
            "isRequired": true,
            "type": "boolean",
            "name": "ParticipationAgreement",
            "title": "Согласие на участие в исследовании:",
            "labelTrue": "Да",
            "labelFalse": "Нет",
        },

    ]
};
let visitId = sessionStorage.getItem("visitId");
window.survey2 = new Survey.Model(json1);

function loadState(survey) {
    $.ajax({
        url:'/api/criteriaforinclusion/'+visitId,
        type: 'GET',
        //data: rest,
        contentType: 'application/json;charset=utf-8',
        success:function (response)
        {
            let res={};
            res=JSON.parse(response);
            if (res.data)
                survey.data = res.data;

        },
        error: function() {
            alert("Возникла ошибка");
        }

    });
    // Here should be the code to load the data from your database
    //var storageSt = window.localStorage.getItem(storageName) || "";
    /*if (storageSt)
        res = JSON.parse(storageSt);*/

    // Create the survey state for the demo. This line should be deleted in the real app. else
    /*res = {
        currentPageNo: 1,
        data: {
            "satisfaction": "4",
            "Quality": {
                "does what it claims": "1"
            },
            "recommend friends": "3",
            "price to competitors": "More expensive",
            "price": "correct",
            "pricelimit": {
                "mostamount": ""
            }
        }
    };*/
    // Set the loaded data into the survey.
}


survey2.onComplete.add(function (sender, options)
{
    survey2.mode = "show";
    survey2.clear(false);
    document.querySelector('#KVkl0res').textContent = "Result JSON:\n" + JSON.stringify(sender.data, null, 3);
    var mySurvey = sender;
    var surveyData = sender.data;
    var xhr = new XMLHttpRequest();
    xhr.open("PUT", "/api/criteriaforinclusion" + visitId, true);
    xhr.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    xhr.onload = xhr.onerror = function () {
        if (xhr.status === 200)
        {
            options.showDataSavingSuccess("Успешный успех"); // you may pass a text parameter to show your own text
            // Or you may clear all messages:
            // options.showDataSavingClear();
        } else
        {
            //Error
            options.showDataSavingError("Бан по причине "+ xhr.status); // you may pass a text parameter to show your own text
        }
    };
    xhr.send(JSON.stringify(sender.data));
});
/*function saveSurveyData(survey) {
    var data = survey.data;
    data.pageNo = survey.currentPageNo;
    window.localStorage.setItem(storageName, JSON.stringify(data));
}*/
/*survey.onComplete.add(function (sender, options) {
    saveSurveyData(sender);
});*/
survey.sendResultOnPageNext = true;

//var prevData = window.localStorage.getItem(storageName) || null;

/*if (prevData) {
    var data = JSON.parse(prevData);
    survey.data = data;
    if (data.pageNo) {
        survey.currentPageNo = data.pageNo;
    }
}*/

/*survey2.data = {
    var data = JSON.parse('api/чётоттам');
};*/
loadState(survey);
$("#KVkl0surv").Survey({model: survey2, onValueChanged: surveyValueChanged});
