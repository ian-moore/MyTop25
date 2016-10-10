import React from 'react';
import classNames from 'classnames'
import Header from './header.jsx'
import ArtistList from './artistlist.jsx'
import DurationPicker from './durationpicker.jsx'

class ArtistView extends React.Component {
    render() {
        var viewClasses = classNames({
            'hidden': !this.props.display
        });
        return (
            <div className={viewClasses}>
                <Header name="Ian" />
                <DurationPicker />
                <ArtistList artists={this.props.artists} />
            </div>
        )
    }
}

export default ArtistView;