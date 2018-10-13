(function () {
    console.log('I am alive')

    fetch("example.json")
        .then(response => {
            return response.json();
        })
        .then(json => {
            renderStalenessChart(json);
            renderAuthorCharts(json);
        });

    var renderStalenessChart = (json) => {
        var labels = [], data = [], colors = []
        json.repositories.forEach(function (repo, index) {
            labels.push(repo.name);
            data.push(daysSince(new Date(repo.latestCommit)));
            colors.push(palette[index % palette.length]);
        });

        var ctx = document.getElementById('staleness-chart').getContext('2d');
        new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    backgroundColor: colors,
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

    var renderAuthorCharts = (json) => {
        var containerEl = document.getElementById('author-charts-container');

        json.repositories.forEach((repo, repoIndex) => {
            var labels = [], data = [], colors = [];

            var authorChartHeader = document.createElement("h3");
            authorChartHeader.innerHTML = repo.name;
            containerEl.appendChild(authorChartHeader);

            var chartId = 'authorChart' + repoIndex;
            var canvasEl = document.createElement("canvas");
            canvasEl.setAttribute("id", chartId);
            containerEl.appendChild(canvasEl);

            repo.contributorCommitCounts.forEach((author, authorIndex) => {
                labels.push(author.key);
                data.push(author.value);
                colors.push(piePalette[authorIndex % piePalette.length]);
            });
            var ctx = document.getElementById(chartId).getContext('2d');
            new Chart(ctx, {
                type: 'doughnut',
                data: {
                    labels: labels,
                    datasets: [{
                        backgroundColor: colors,
                        data: data,
                    }]
                },
                options: {
                    legend: {
                        position: 'left'
                    }
                }
            });
        });
    }

    var daysSince = (date) => {
        return moment().diff(moment(date), "days")
    }

    var palette = ["#ffb6b9", "#fae3d9", "#bbded6", "#61c0bf", "#aaaaaa"];
    var piePalette = ["#bbded6", "#61c0bf", "#aaaaaa", "#ffb6b9", "#fae3d9"]
})();