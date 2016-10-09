import React from 'react';
import classNames from 'classnames'

class LoadingIndicator extends React.Component {
    render() {
        var loadingClass = classNames({
            'hidden': !this.props.display
        });
        return (
            <div className={loadingClass}>Loading...</div>
        )
    }
}

export default LoadingIndicator;