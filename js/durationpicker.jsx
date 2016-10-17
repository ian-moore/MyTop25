import React from 'react';

class DurationPicker extends React.Component {
    render() {
        return (
            <div className="duration-picker">
                <input type="radio" id="duration-short" name="duration" value="short_term" onChange={this.props.onChange} />
                <label htmlFor="duration-short">Short Term</label>
                <input type="radio" id="duration-medium" name="duration" value="medium_term" onChange={this.props.onChange} defaultChecked />
                <label htmlFor="duration-medium">Medium Term</label>
                <input type="radio" id="duration-long" name="duration" value="long_term" onChange={this.props.onChange} />
                <label htmlFor="duration-long">Long Term</label>
            </div>
        )
    }
}

export default DurationPicker;