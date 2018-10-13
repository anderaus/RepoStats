# RepoStats

Simple .NET Core console application for collecting statistics from a set of remote git repositories. Meant used for developer KPI reporting.

## Features

- [x] Most recent commit for each repo (repo staleness)
- [x] Number of commits per contributor
- [ ] Number of unique contributors last X months (human coverage)
- [ ] Any suggestions?

## How to use

Clone, build, edit `appsettings.json` and run:

`dotnet run --username {username} --password {password}`

## Configuration

Example `appsettings.json` content:

```json
{
  "username": "{username}",
  "password":  "{password}", 
  "repos": [
    {
      "url": "http://something.something.git",
      "friendlyName": "My fine nuget"
    },
    {
      "url": "http://otherthing.otherthing.git",
      "friendlyName": "My fine service"
    }
  ],
  "authorEmailAliases": [
    {
      "primary": "post@andersaustad.com",
      "aliases": [ "anderaus@gmail.com", "anders.austad@novanet.no", "anderaus@outlook.com" ]
    }
  ]
}
```

## Status

Incomplete proof-of-concept using [libgit2](https://github.com/libgit2/libgit2sharp). Currently clones/fetches repos, loops through commits and collects some key stats. Simple visualization of staleness and coverage using chart.js.

### Todos

- [x] Read auth and repos info from config file or command line
- [x] Clone/fetch remote repos
- [x] Gather basic statistics from repos
- [ ] Gather advanced statistics from repos
- [x] Use configured email aliases to find unique contributors (some contribute using multiple emails)
- [x] Persist key statistics to json file
- [x] Create basic static html page visualizing the result
- [ ] Create more advanced html page visualizing the result using chart.js