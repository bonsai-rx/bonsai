import WorkflowContainer from "./workflow.js"

export default {
    defaultTheme: 'light',
//#if (HasGitHubSlug)
    iconLinks: [{
        icon: 'github',
        href: 'https://github.com/$ghslug$',
        title: 'GitHub'
    }],
//#endif
    start: () => {
        WorkflowContainer.init();
    }
}