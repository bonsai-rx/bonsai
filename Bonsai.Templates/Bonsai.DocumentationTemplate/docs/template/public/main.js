import WorkflowContainer from "./workflow.js"

export default {
    defaultTheme: 'light',
//#if (HasGitHubRepo)
    iconLinks: [{
        icon: 'github',
        href: '$repourl$',
        title: 'GitHub'
    }],
//#endif
    start: () => {
        WorkflowContainer.init();
    }
}