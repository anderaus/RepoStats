(function () {
    console.log('I am alive')

    fetch("example.json")
        .then(response => {
            return response.json();
        })
        .then(json => {
            renderGraph(json);
        });

    var renderGraph = (json) => {
        var labels = [], data = [], colors = []
        json.repositories.forEach(function (repo, index) {
            labels.push(repo.name);
            data.push(daysSince(new Date(repo.latestCommit)));
            colors.push(palette[index % palette.length]);
        });

        var ctx = document.getElementById('chart').getContext('2d');
        new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    backgroundColor: colors,
                    borderColor: 'rgb(255, 255, 255)',
                    data: data,
                }]
            },
            options: {
                legend: {
                    display: false
                }
            }
        });
    }

    var daysSince = (date) => {
        return moment().diff(moment(date), "days")
    }

    var palette = ["#ffb6b9", "#fae3d9", "#bbded6", "#61c0bf", "#aaaaaa"];
})();