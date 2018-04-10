$(function () {
    var myChart = null;
    getMonthsBudgeted();
    var yearSelect = $('.month').val();
    var yearSelect = $('.year').val();
    var categorypicked = $('#category-picked').val();
    totalAvailIncome();

    // event handlers

    // shows model to add month
    $('#exampleModal').click(function () {

        $('#new-month-model').show();

    });
    // adds month for user
    $('#addMonthButton').click(function () {
        var month = {
            Month: $('.add-month').val(),
            Year: $('.add-year').val(),
            UserId: $('.userId').val()
        };

        
       
        $.post('/Budget/AddMonth', month, function (newMonthId) {
            $('#exampleModal').modal('hide');
                 
            if (newMonthId == 0) {

                alert('You already started budgeting for ' + month.Month + ' ' + month.Year);
                
            } else {
                getMonthsBudgeted(true);
                month.MonthNum = $('.add-month').find('option:selected').data('monthnumber');
           
                switchMonthBudgeting(newMonthId, `${month.Month} ${month.Year}`, month.MonthNum, month.Year);
                $('#available-income').show();
                $('#debit-table tr:gt(0)').remove();
                $('#credits-table tr:gt(0)').remove();

            }
            
        });
    });

    // switch month
    $('.months-budgeted').on('click', '.month', function () {
        defaultDebitTableDropDown();
 
        //passing in the function (db month id, month name and year e.g. December 2017, month number e.g. jan = 1, year )
        switchMonthBudgeting($(this).data('monthid'), $(this).text(), $(this).data('monthnum'), $(this).data('year'));

         getCategoriesWithSumsForMonths($(this).data('monthid'));

        $.get('/Budget/SwitchMonth', { NewMonthId: $(this).data('monthid') }, function (result) {
        
            addCreditToTable(result.credits);
            addDebitToTable(result.debits);
            $('#total-credit').text(result.creditTotal)
            addDebitSum(result.debitTotal);
           

        });


    });

  
    // adds credit 
    $('#add-credit').click(function () {
        var newCredit = {
            Amount: $('#creditInputAmount').val(),
            Source: $('#credit-source').val(),
            Date: $('#creditInputDate').val(),
            MonthId: $('#month-id').val()
        };

        if (newCredit.Amount === '') {
            alert('Please enter an amount.');
            return;
        }
        if (newCredit.Source === '') {
            alert('Please enter source of income.');
            return;
        }
        var year = newCredit.Date.substring(0, 4);
        var month = newCredit.Date.substring(5, 7);

        if (!isDateWithinMonth(month, year)) {
            alert('The date of the expense is not in ' + $('#month-year-showring').text());
            return;
        }
        $.post('/budget/addCredit', newCredit, function (result) {

       
            addCreditToTable(result.credits)
            addCreditSum(result.creditTotal);
            totalAvailIncome();
        });
     
       
        returnInputToDefault();

    });

    // adds debit
    $('#add-debit').click(function () {
  
        var newDebit = {
            Amount:  $('#debitInputAmount').val(),
            Details: $('#details').val(),
            Date: $('#debitInputDate').val(),
            CategoryId: $('#debit-category').val(),
            MonthId: $('#month-id').val()
        }

        if (newDebit.CategoryId == 0) {
            alert("please pick a category");
   
            return;
        }
        if (newDebit.Amount === '' || newDebit.Details === null || newDebit.Date === '') {
            alert("Please fill out all fields");

            return;
        }
        var year = newDebit.Date.substring(0, 4);
        var month = newDebit.Date.substring(5, 7);
        if (!isDateWithinMonth(month, year)) {
            alert('The date of the expense is not in ' + $('#month-year-showring').text());
            return;
        }
       

        $.post('/Budget/AddDebit', newDebit, function (result) {
     
           
            addDebitToTable(result.debits);
            addDebitSum(result.debitTotal);
            totalAvailIncome();
            getCategoriesWithSumsForMonths(newDebit.MonthId);
        });
        returnInputToDefault();
        
    });

    // remove credit from table

    $('#credit-table').on('click', '.delete-credit-button', function () {
        var tr = $(this).closest('tr');
     
        tr.remove();

    });


    //gets the selected value when user changes category in debit table
    $('#category').change(function () {
      
        showSelectedDebitRows($(this).val());

    });

    
    //functions 

    // builds the HTML for a new month
    function buildMonthHTML(month, newMonthId) {
        var html = `<li class="month"  data-monthid="${newMonthId}" data-monthnum="${month.MonthNum }" data-year="${month.Year}"><a> ${month.Month} ${month.Year}</a> </li>`
        return html;
    }

    // writes out the Month and Year and gives the hidden input a month value
  
    function switchMonthBudgeting(monthId, monthNameAndYear, monthNum, year) {
        $('#month-id').val(monthId);
        $('.month-year-showring').text(monthNameAndYear);
        $('.transaction-month').show();
        setDatePicker(monthNum, year);
        
    }

    // sets date picker to first day of current viewed month
    function setDatePicker(monthNum, year) {
        monthNum = monthNum + 1;
        if (monthNum < 10) {
            monthNum = '0' + monthNum;
        }
        $('.date-range').val(year + '-' + monthNum + '-01');
        $('#month-id').data('currentmonth', monthNum);
        $('#month-id').data('curentyear', year);
       
    }

    // once income or an expense is added this function returns the inputs to default values
    function returnInputToDefault() {
        $('.amount').val("");
        $('#credit-source').val("");
        $('#details').val("");
        $('#category-picked').val(categorypicked);
    }

   
    // adds income to table for current viewed month

    function addCreditToTable(credits) {
        $('#credits-table tr:gt(0)').remove();

        credits.forEach(credit => {
            var html = `
         <tbody> <tr>
                  <td>${credit.Amount}</td>
                  <td>${formatMVCDate(credit.Date)}</td>
                  <td>${credit.Source}</td>
                  <td><button data-creditId="${credit.Id}" class ="btn btn-primary delete-credit-button">Delete</button></td>
            </tr></tbody>
            `
            $('#credits-table').append(html);
        });
    }

    // adds expenses to table for current viewed month
    function addDebitToTable(debits) {

        $('#debit-table tr:gt(0)').remove();
        debits.forEach( debit => {
            var html = `

            <tbody>  <tr>
                    <td>${debit.amount}</td>
                    <td>${formatMVCDate(debit.date)} </td>
                    <td>${debit.category}</td>
                    <td>${debit.detail}</td>
                </tr></tbody>
            `
            $('#debit-table').append(html);
    });
    
    }


    // gets all months budgeted for user. if false  is passed in then it will not call setDatePicker. 

    function getMonthsBudgeted(x) {
        $('.months-budgeted li').remove();
        $.get('/Budget/GetMonthsBudgeted', function (result) {
            if (result.length === 0) {
                return;
            }
            result.forEach(M => $('.months-budgeted').append(buildMonthHTML(M, M.Id)));
            var lastMonth = result[result.length - 1];
            getCategoriesWithSumsForMonths(lastMonth.Id);
            if (!x) {
                setDatePicker(lastMonth.MonthNum, lastMonth.Year);
            }


        });
    }

    //adds sum of credits for month. This is called when a month is switched or a new credit is added. This will then call totalAvailIncome to update the available income for the month
    function addCreditSum(creditSum) {
        $('.transaction-month').show();
        $('#total-credit').text(creditSum);
        totalAvailIncome();
    }

    //adds sum of debit for month. This is called when a month is switched or a new credit is added. This will then call totalAvailIncome to update the available income for the month

    function addDebitSum(debitSum) {
        $('.transaction-month').show();
        $('#total-expense').text(debitSum);
        totalAvailIncome();
    }

    //parses number to $ currency
    function parseNumberToCurrency(n) {
        return '$' + " " + n.toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, "$1,");
    }

    // when user tries to add income or an expense this function checks if the date is within the current month
    function isDateWithinMonth(month, year) {
        var currentMonth = $('#month-id').data('currentmonth');
        var currentYear = $('#month-id').data('curentyear');
     
        if (currentMonth == month && currentYear == year) {
            return true;
        }
        else {
            return false;
        }

    }

    //  gets the total available income for the current viewed month. 
    function totalAvailIncome() {
        var creditTotal = $('#total-credit').text().substring(1);
        var debitTotal = $('#total-expense').text().substring(1);

        creditTotal = removeCommas(creditTotal);
        debitTotal = removeCommas(debitTotal);

        var availableIncome = creditTotal - debitTotal;
        $('#total-available').text(parseNumberToCurrency(creditTotal - debitTotal));
        if (availableIncome === 0) {
            $('#total-available').text('You have no available income for ' + $('#month-year-showring').text());
        }
        else if (availableIncome > 0) {
            $('#total-available').text('your available income for the month of ' + $('#month-year-showring').text() + ' is ' + parseNumberToCurrency(availableIncome));
            $('#total-available').css('color', 'green');

        }
        else {
            $('#total-available').text('For the month of ' + $('#month-year-showring').text() + ' you have a negative balance in the amount of ' + parseNumberToCurrency(availableIncome));
            $('#total-available').css('color', 'red');
        }
    }

    //removes commas from so that math can be done on the numbers
    function removeCommas(str) {
        return (str.replace(/,/g, ''));
    }

    // This function gets all categories with total amount spent for a month and passes it into the fillPie function
    function getCategoriesWithSumsForMonths(monthId) {

        $.get('/Budget/GetAmountForCategory', { MonthId: monthId }, function (result) {

            fillPie(result);
            return result;
        }
    );
    }

    function fillPie(result) {

        // if a pie chart was already created this will destroy that chart so that there should be no reference to it anymore
        if (myChart != null) {
            myChart.destroy();
        }
        var ctx = document.getElementById('myPieChart').getContext('2d');
        myChart = new Chart(ctx, {
            type: 'pie',
            data: {
                labels: [result[0].categoryName, result[1].categoryName, result[2].categoryName, result[3].categoryName, result[4].categoryName, result[5].categoryName, result[6].categoryName, result[7].categoryName, result[8].categoryName],
                datasets: [{
                    backgroundColor: [
                      "#2ecc71",
                      "#3498db",
                      "#95a5a6",
                      "#9b59b6",
                      "#f1c40f",
                      "#e74c3c",
                      "#34495e",
                      "#DECF3F",
                      "#B276B2",
                      "#B2912F"
                    ],
                    data: [result[0].amount, result[1].amount, result[2].amount, result[3].amount, result[4].amount, result[5].amount, result[6].amount, result[7].amount, result[8].amount]
                }]
            }
        });
    }

    //only shows rows  from debit table that are un the user selected category

    function showSelectedDebitRows(selectedeCategory) {     // i still want to add the option of getting the total of the categorie selected

        // $('#debit-table tr').each(function (row, tr) {
        var total = Number();

        $('#debit-table tr:gt(0)').each(function () {
            $(this).show();
        });

        if (selectedeCategory === 'allCategories') {
            defaultDebitTableDropDown();
            return;
        }


        $('#debit-table tr:gt(0)').each(function () {
            var categoryForRow = $(this).find('td:eq(2)').text();
            if (categoryForRow != selectedeCategory) {
                $(this).hide();
            }
                //the below add the total for the selected category
            else {
                var amount = $(this).find('td:eq(0)').text().substring(1);
                amount = removeCommas(amount);

                total = +total + +amount;
            }

        });
        showTotalForCategory(selectedeCategory,total);
       
    }


    // i want to show the total for category. then create  a new function that will unhide all rows and hide the total for the category
    function showTotalForCategory(selectedCategory, total) {
        $('#totalCategoryName').text(selectedCategory);
        $('#totalCategoryAmount').text(parseNumberToCurrency(total));
        $('#totalForCategory').show();
       

        console.log(total);

    }

    // $('#category')
    function defaultDebitTableDropDown() {
        $('#category option[value="allCategories"]').attr("selected", true);
        hideTotalForCategory();
    }

    function hideTotalForCategory() {
        $('#totalCategoryName').text('');
        $('#totalCategoryAmount').text('');
        $('#totalForCategory').hide();

    }

    $('#parseTable').click(function () {
        console.log('clicked parse button');

        console.log(getDebiTableInArray());
    })

    //i do not use this function. this is just to to get all cells in the debit table into an array
    function getDebiTableInArray() {

        var debitTableData = [];

        $('#debit-table tr').each(function (row, tr) {

            var line = {
                amount: $(tr).find('td:eq(0)').text(),
                date: $(tr).find('td:eq(1)').text(),
                category: $(tr).find('td:eq(2)').text(),
                detail: $(tr).find('td:eq(3)').text()
            }
            debitTableData.push(line);
          

        });

        //removes first row in array
        debitTableData.shift();

        return debitTableData;

    }

});



