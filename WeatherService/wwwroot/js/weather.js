"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/weatherHub").build();

connection.start().then(function () {
    //
})
    .catch(function (err) {
        return console.error(err.toString());
    });

function updateDayNumberText(val)
{
    document.getElementById("dayNumberText").innerText = val;
}


document.getElementById("dataSrcSelect").style.display = "block";
document.getElementById("dataSrcLabel").style.display = "block";
document.getElementById("citySelectLabel").style.display = "none";
document.getElementById("citySelect").style.display = "none";

//document.getElementById("weatherTable").style.display = "none";

function checkSelectedDataType(that)
{
    if (that.value == "stdDeviationValue")
    {
        document.getElementById("dataSrcLabel").style.display = "none";
        document.getElementById("dataSrcSelect").style.display = "none";
        //document.getElementById("dataSrcSelect").value = null;

        document.getElementById("citySelectLabel").style.display = "block";
        document.getElementById("citySelect").style.display = "block";
    }
    else
    {
        document.getElementById("dataSrcLabel").style.display = "block";
        document.getElementById("dataSrcSelect").style.display = "block";

        document.getElementById("citySelectLabel").style.display = "none";
        document.getElementById("citySelect").style.display = "none";
        //document.getElementById("citySelect").value = null;
    }
}



document.getElementById("submitBtn").addEventListener("click", function (event)
{
    //alert("soso btn cliecked!");

    var obj = new Object();
    obj.dataType = document.getElementById("dataTypes").value;
    obj.dataSrc = document.getElementById("dataSrcSelect").value;
    obj.city = document.getElementById("citySelect").value;
    obj.daysNumber = +document.getElementById("daysNumberInput").value;

    connection.invoke("SendNewWeatherData", obj)
                .catch(function (err) {
                    return console.error(err.toString());
                });
    
    event.preventDefault();
});



connection.on("newCurrDataWeather", function (resultData, daysNumber) 
{
    var str = JSON.stringify(resultData);
    alert("Curr - otrzymalem dane z serwera:" + str);

    createCurrAvgWeatherTable(resultData, "currentData", daysNumber);

});

connection.on("newAvgDataWeather", function (resultData, daysNumber) 
{
    var str = JSON.stringify(resultData);
    alert("AVG - otrzymalem dane z serwera:" + str);

    createCurrAvgWeatherTable(resultData, "avgData", daysNumber);
});

function createCurrAvgWeatherTable(resultData, dataType, daysNumber)
{
    var table = document.getElementById("weatherCurrAvgTable");
    table.innerHTML = "";

    if (resultData == null)
    {
        alert("no data for this option!");
        table.innerHTML = "No data for this option!";
        return;
    }

    var weatherServiceName = resultData[0].weatherServiceName;

    var thead = document.createElement("thead");
    var tbody = document.createElement("tbody");
    
    //pierwszy wiersz z nazwa tabeli
    var theadNameRow = document.createElement("tr");
    var theadNameCell = document.createElement("th");
    theadNameCell.innerHTML = "Service: " + weatherServiceName;
    theadNameRow.appendChild(theadNameCell);
    thead.appendChild(theadNameRow);

    //wiersz z nazwami miast
    var theadRow = document.createElement("tr");
    var labelCell = document.createElement("th"); //[?Label?] - current date OR average of {var} days
    if (dataType == "currentData")
        labelCell.innerHTML = "Current " + resultData[0].dateTime; 
    else if (dataType == "avgData")
        labelCell.innerHTML = "Average of " + daysNumber + " days"; 
    else
        labelCell.innerHTML = "Error";
    theadRow.appendChild(labelCell);
    tbody.appendChild(theadRow);

    //pionowa kolumna z etykietami danych tj. temperature, pressure itd.
    //wiersz temperatury
    var tempRow = document.createElement("tr");
    var tempLbl = document.createElement("th");
    tempLbl.innerHTML = "temperature [Celsius dg]";
    tempRow.appendChild(tempLbl);
    tbody.appendChild(tempRow);

    ////wiersz cisnienia
    var pressRow = document.createElement("tr");
    var pressLbl = document.createElement("th");
    pressLbl.innerText = "pressure [hPa]";
    pressRow.appendChild(pressLbl);
    tbody.appendChild(pressRow);

    ////wiersz humidity
    var humidityRow = document.createElement("tr");
    var humidityLbl = document.createElement("th");
    humidityLbl.innerText = "humidity [%]";
    humidityRow.appendChild(humidityLbl);
    tbody.appendChild(humidityRow);

    ////wiersz precipitation [mm]
    var rainRow = document.createElement("tr");
    var rainLbl = document.createElement("th");
    rainLbl.innerText = "precipitation [mm]";
    rainRow.appendChild(rainLbl);
    tbody.appendChild(rainRow);

    ////wiersz windSpeed
    var windSpeedRow = document.createElement("tr");
    var windSpeedLbl = document.createElement("th");
    windSpeedLbl.innerText = "wind speed [m/s]";
    windSpeedRow.appendChild(windSpeedLbl);
    tbody.appendChild(windSpeedRow);

    ////wiersz windDirection
    var windDirRow = document.createElement("tr");
    var windDirLbl = document.createElement("th");
    windDirLbl.innerText = "wind direction [dg]";
    windDirRow.appendChild(windDirLbl);
    tbody.appendChild(windDirRow);

    for (var i = 0; i < resultData.length; i++)
    {
        var cityData = resultData[i];

        var theadCell = document.createElement("th");
        theadCell.innerHTML = cityData.city;
        theadRow.appendChild(theadCell);

        var tempData = document.createElement("td");
        tempData.innerHTML = cityData.temperature;
        tempRow.appendChild(tempData);

        var pressData = document.createElement("td");
        pressData.innerHTML = cityData.pressure;
        pressRow.appendChild(pressData);

        var humidityData = document.createElement("td");
        humidityData.innerHTML = cityData.humidity;
        humidityRow.appendChild(humidityData);

        var rainData = document.createElement("td");
        rainData.innerHTML = cityData.rain;
        rainRow.appendChild(rainData);

        var windSpeedData = document.createElement("td");
        windSpeedData.innerHTML = cityData.windSpeed;
        windSpeedRow.appendChild(windSpeedData);

        var windDirData = document.createElement("td");
        windDirData.innerHTML = cityData.windDirection;
        windDirRow.appendChild(windDirData);

        //alert("Done!");
    }


    table.appendChild(thead);
    table.appendChild(tbody);

}

connection.on("newStdDeviationDataWeather", function (resultData, daysNumber) 
{
    var str = JSON.stringify(resultData);
    alert("StdDeviat - otrzymalem dane z serwera:" + str);

    createStdDeviationWeatherTable(resultData,daysNumber);

});

function createStdDeviationWeatherTable(resultData, daysNumber)
{
    var table = document.getElementById("weatherCurrAvgTable");
    table.innerHTML = "";

    if (resultData == null)
    {
        alert("no data for this option!");
        return;
    }

    var cityName = resultData[0].city;

    var thead = document.createElement("thead");
    var tbody = document.createElement("tbody");

    //wiersz z nazwami miast
    var theadRow = document.createElement("tr");
    var labelCell = document.createElement("th"); //[?Label?] - current date OR average of {var} days
    labelCell.innerHTML = "City: " + cityName;
    theadRow.appendChild(labelCell);
    tbody.appendChild(theadRow);

    //pionowa kolumna z etykietami danych tj. temperature, pressure itd.
    //wiersz temperatury
    var tempRow = document.createElement("tr");
    var tempLbl = document.createElement("th");
    tempLbl.innerHTML = "temperature [Celsius dg]";
    tempRow.appendChild(tempLbl);
    tbody.appendChild(tempRow);

    ////wiersz cisnienia
    var pressRow = document.createElement("tr");
    var pressLbl = document.createElement("th");
    pressLbl.innerText = "pressure [hPa]";
    pressRow.appendChild(pressLbl);
    tbody.appendChild(pressRow);

    ////wiersz humidity
    var humidityRow = document.createElement("tr");
    var humidityLbl = document.createElement("th");
    humidityLbl.innerText = "humidity [%]";
    humidityRow.appendChild(humidityLbl);
    tbody.appendChild(humidityRow);

    ////wiersz precipitation [mm]
    var rainRow = document.createElement("tr");
    var rainLbl = document.createElement("th");
    rainLbl.innerText = "precipitation [mm]";
    rainRow.appendChild(rainLbl);
    tbody.appendChild(rainRow);

    ////wiersz windSpeed
    var windSpeedRow = document.createElement("tr");
    var windSpeedLbl = document.createElement("th");
    windSpeedLbl.innerText = "wind speed [m/s]";
    windSpeedRow.appendChild(windSpeedLbl);
    tbody.appendChild(windSpeedRow);

    ////wiersz windDirection
    var windDirRow = document.createElement("tr");
    var windDirLbl = document.createElement("th");
    windDirLbl.innerText = "wind direction [dg]";
    windDirRow.appendChild(windDirLbl);
    tbody.appendChild(windDirRow);

    for (var i = 0; i < resultData.length; i++)
    {
        var wthServData = resultData[i];

        var theadCell = document.createElement("th");

        if (i == resultData.length - 1)
        {
            theadCell.innerHTML = "Standard deviation " + daysNumber + " days";
            theadRow.appendChild(theadCell);
        }
        else
        {
            theadCell.innerHTML = wthServData.weatherServiceName;
            theadRow.appendChild(theadCell);
        }

        var tempData = document.createElement("td");
        tempData.innerHTML = wthServData.temperature;
        tempRow.appendChild(tempData);

        var pressData = document.createElement("td");
        pressData.innerHTML = wthServData.pressure;
        pressRow.appendChild(pressData);

        var humidityData = document.createElement("td");
        humidityData.innerHTML = wthServData.humidity;
        humidityRow.appendChild(humidityData);

        var rainData = document.createElement("td");
        rainData.innerHTML = wthServData.rain;
        rainRow.appendChild(rainData);

        var windSpeedData = document.createElement("td");
        windSpeedData.innerHTML = wthServData.windSpeed;
        windSpeedRow.appendChild(windSpeedData);

        var windDirData = document.createElement("td");
        windDirData.innerHTML = wthServData.windDirection;
        windDirRow.appendChild(windDirData);

        //alert("Done!");
    }

    table.appendChild(thead);
    table.appendChild(tbody);

}


