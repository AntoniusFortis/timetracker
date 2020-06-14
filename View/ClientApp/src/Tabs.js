import React from 'react';

export function Tabs({ children, selectedTab, onChangeTab }) {
    let tabProps = []
    const content = React.Children.map(children, (child) => {
        if (child.type === Tab) {
            const { title, name } = child.props
            tabProps.push({ title, name })
            if (selectedTab ? (selectedTab !== child.props.name) : (tabProps.length !== 1)) {
                return null
            }
        }
        return child
    })

    const finalSelectedTab = selectedTab ||
        (tabProps.length > 0 && tabProps[0].name)

    return (
        <div className="tabs">
            <Tablist selectedTab={finalSelectedTab}
                onChangeTab={onChangeTab}
                tabs={tabProps}/>
            <div className="tabs__content">
                {content}
            </div>
        </div>
    )
}

export function Tab({ name, children }) {
    return (
        <div id={`tab-${name}`} className="tabs__tab">
            {children}
        </div>
    )
}

function Tablist({ tabs, selectedTab, onChangeTab }) {
    const linkClass = selected => {
        const c = 'tabs__tablist__link'
        return selected ? `${c} ${c}--selected` : c
    }

    return (
        <menu className="tabs__tablist">
            <ul>
                {tabs.map(({ name, title }) =>
                    <li aria-selected={name === selectedTab} role="tab" key={name}>
                        <a className={linkClass(name === selectedTab)} onClick={() => onChangeTab(name)}>
                            {title}
                        </a>
                    </li>
                )}
            </ul>
        </menu>
    )
}